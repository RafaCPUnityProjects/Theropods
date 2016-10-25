/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"Action.cs"
 * 
 *	This is the base class from which all Actions derive.
 *	We need blank functions Run, ShowGUI and SetLabel,
 *	which will be over-ridden by the subclasses.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{
	
	[System.Serializable]
	abstract public class Action : ScriptableObject
	{
		
		public bool willWait;
		public float defaultPauseTime = 0.1f;
		
		public bool isRunning;
		public int id;
		
		public bool isDisplayed;
		public string title;
		
		public enum ResultAction { Continue, Stop, Skip, RunCutscene }
		public ResultAction endAction = ResultAction.Continue;
		public int skipAction;
		public AC.Action skipActionActual;
		public Cutscene linkedCutscene;
		
		public bool isMarked = false;
		public bool isEnabled = true;
		
		
		public Action ()
		{
			this.isDisplayed = true;
		}
		
		
		public virtual float Run ()
		{
			return defaultPauseTime;
		}
		
		
		public virtual void ShowGUI () {}
		
		
		public virtual int End (List<Action> actions)
		{
			if (endAction == ResultAction.Stop)
			{
				return -1;
			}
			else if (endAction == ResultAction.Skip)
			{
				int skip = skipAction;
				if (skipActionActual && actions.IndexOf (skipActionActual) > 0)
				{
					skip = actions.IndexOf (skipActionActual);
				}

				return (skip);
			}
			else if (endAction == ResultAction.RunCutscene && linkedCutscene)
			{	
				return -1;
			}
			
			// Continue as normal
			return 0;
		}
		
		
		#if UNITY_EDITOR
		
		protected void AfterRunningOption ()
		{		
			endAction = (ResultAction) EditorGUILayout.EnumPopup ("After running:", (ResultAction) endAction);
			
			if (endAction == ResultAction.RunCutscene)
			{
				linkedCutscene = (Cutscene) EditorGUILayout.ObjectField ("Cutscene to run", linkedCutscene, typeof (Cutscene), true);
			}
		}
		
		
		public virtual void SkipActionGUI (List<Action> actions, bool showGUI)
		{
			int tempSkipAction = skipAction;
			int offset = actions.IndexOf (this) + 1;
			List<string> labelList = new List<string>();
			
			if (skipActionActual)
			{
				bool found = false;

				if (offset <= actions.Count)
				{
					for (int i = 0; i < actions.Count - offset; i++)
					{
						labelList.Add ((i + offset).ToString () + ": " + actions [i + offset].title);

						if (skipActionActual == actions [i + offset])
						{
							skipAction = i + offset;
							found = true;
						}
					}
				}
				
				if (!found)
				{
					skipAction = tempSkipAction;
				}
			}

			if (skipAction < offset)
			{
				skipAction = offset;
			}
			
			if (skipAction >= actions.Count)
			{
				if (offset == actions.Count)
				{
					skipAction = 0;
				}
				else
				{
					skipAction = actions.Count - 1;
				}
			}

			if (showGUI)
			{
				if (skipAction > 0)
				{
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField ("  Action to skip to:");
					tempSkipAction = EditorGUILayout.Popup (skipAction - offset, labelList.ToArray());
					EditorGUILayout.EndHorizontal();
					skipAction = tempSkipAction + offset;
					skipActionActual = actions [skipAction];
				}
				else
				{
					EditorGUILayout.HelpBox ("Cannot skip action - no further Actions available", MessageType.Warning);
				}
			}
			else
			{
				if (skipAction > 0)
				{
					skipActionActual = actions [skipAction];
				}
			}
		}


		public virtual string SetLabel ()
		{
			return ("");
		}

		#endif
		
	}
	
}
