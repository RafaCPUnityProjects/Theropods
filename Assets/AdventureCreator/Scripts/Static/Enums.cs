/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"Enums.cs"
 * 
 *	This script containers any enum type used by more than one script.
 * 
 */

namespace AC
{
	public enum GameState { Normal, Cutscene, DialogOptions, Paused };
	public enum ActionListType { PauseGameplay, RunInBackground };

	public enum AppearType { Manual, MouseOver, DuringConversation, OnInputKey, OnInteraction, OnHotspot, WhenSpeechPlays, DuringGameplay };
	public enum MenuTransition { Fade, Pan, FadeAndPan, Zoom };
	public enum PanDirection { Up, Down, Left, Right };
	public enum PanMovement { Linear, Smooth, Overshoot };
	public enum MenuOrientation { Horizontal, Vertical };
	public enum ElementOrientation { Horizontal, Vertical, Grid };
	public enum AC_PositionType { Centred, Aligned, Manual, FollowCursor, AppearAtCursorAndFreeze, OnHotspot, AboveSpeakingCharacter };
	public enum AC_PositionType2 { Aligned, Manual };
	public enum AC_ShiftInventory { ShiftLeft, ShiftRight };
	public enum AC_SizeType { Automatic, Manual };
	public enum AC_InputType { AlphaNumeric, NumbericOnly };
	public enum AC_LabelType { Normal, Hotspot, DialogueLine, DialogueSpeaker, DialoguePortrait, Variable };
	public enum AC_SaveListType { Save, Load };
	public enum AC_ButtonClickType { TurnOffMenu, Crossfade, OffsetInventory, RunActionList, CustomScript, OffsetJournal, SimulateInput };
	public enum SimulateInputType { Button, Axis };
	public enum AC_SliderType { Speech, Music, SFX, CustomScript };
	public enum AC_CycleType { Language, CustomScript };
	public enum AC_ToggleType { Subtitles, CustomScript };
	public enum AC_InventoryBoxType { Default, HostpotBased, CustomScript };
	public enum ConversationDisplayType { TextOnly, IconOnly };

	public enum AC_TextType { Speech, Hotspot, DialogueOption, InventoryItem, CursorIcon, MenuElement, HotspotPrefix, JournalEntry };
	public enum CursorDisplay { Always, OnlyWhenPaused, Never };
	public enum LookUseCursorAction { DisplayBothSideBySide, DisplayUseIcon };

	public enum InteractionType { Use, Examine, Inventory };
	public enum AC_InteractionMethod { ContextSensitive, ChooseInteractionThenHotspot, ChooseHotspotThenInteraction };
	public enum HotspotDetection { MouseOver, PlayerVicinity };
	public enum PlayerAction { DoNothing, TurnToFace, WalkTo, WalkToMarker };
	public enum CancelInteractions { CursorLeavesMenus, ClickOffMenu };
	public enum InventoryInteractions { ChooseInventoryThenInteraction, ContextSensitive };

	public enum AnimationEngine { Legacy, Sprites2DToolkit, SpritesUnity, Mecanim };
	public enum TalkingAnimation { Standard, CustomFace };
	public enum MovementMethod { PointAndClick, Direct, FirstPerson, Drag };
	public enum InputMethod { MouseAndKeyboard, KeyboardOrController, TouchScreen };
	public enum DirectMovementType { RelativeToCamera, TankControls };
	public enum CameraPerspective { TwoD, TwoPointFiveD, ThreeD };
	public enum MovingTurning { WorldSpace, ScreenSpace, TopDown };
	public enum DragAffects { Movement, Rotation };

	public enum InteractionIcon { Use, Examine, Talk };
	public enum InventoryHandling { ChangeCursor, ChangeHotspotLabel, ChangeCursorAndHotspotLabel };

	public enum CharState { Idle, Custom, Move, Decelerate };
	public enum AC_2DFrameFlipping { None, LeftMirrorsRight, RightMirrorsLeft };
	public enum FadeType { fadeIn, fadeOut };
	public enum SortingMapType { SortingLayer, OrderInLayer };

	public enum CameraLocConstrainType { TargetX, TargetZ, TargetAcrossScreen, TargetIntoScreen, SideScrolling };
	public enum CameraRotConstrainType { TargetX, TargetZ, TargetAcrossScreen, TargetIntoScreen, LookAtTarget };

	public enum MoveMethod { Linear, Smooth, Curved };

	public enum AnimLayer {	Base=0, UpperBody=1, LeftArm=2, RightArm=3, Neck=4, Head=5, Face=6, Mouth=7 };
	public enum AnimStandard { Idle, Walk, Run, Talk };
	public enum AnimPlayMode { PlayOnce=0, PlayOnceAndClamp=1, Loop=2 };
	public enum AnimPlayModeBase { PlayOnceAndClamp=1, Loop=2 };
	public enum AnimMethodCharMecanim { ChangeParameterValue, SetStandard };
	public enum MecanimCharParameter { MoveSpeedFloat, TalkBool, TurnFloat };
	public enum MecanimParameterType { Float, Int, Bool, Trigger };

	public enum PlayerMoveLock { Free=0, AlwaysWalk=1, AlwaysRun=2, NoChange=3 };
	public enum AC_OnOff { On, Off };
	public enum TransformType { Translate, Rotate, Scale };

	public enum VariableType { Boolean, Integer, String };
	public enum BoolValue { True=1, False=0 };
	public enum SetVarMethod { SetValue, IncreaseByValue, SetAsRandom };
	public enum SetVarMethodString { EnteredHere=0, SetAsMenuInputLabel=1 };
	public enum SetVarMethodIntBool { EnteredHere=0, SetAsMecanimParameter=1 };

	public enum AC_Direction { None, Up, Down, Left, Right };
	public enum ArrowPromptType { KeyOnly, ClickOnly, KeyAndClick };

	public enum AC_NavigationMethod { UnityNavigation, meshCollider };
	public enum AC_PathType { Loop, PingPong, ForwardOnly, IsRandom };
	public enum PathSpeed { Walk=0, Run=1 };

	public enum SoundType { SFX, Music, Other };

}