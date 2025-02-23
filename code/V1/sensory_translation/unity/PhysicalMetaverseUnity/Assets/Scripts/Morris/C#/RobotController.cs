using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Linq;

// Script used by Odile robots, it lets you control with Keyboard or F710 Logitec Gamepad, also has a gui to change controller button mapping
public class RobotController : MonoBehaviour
{
    public bool _guiEnabled = true;
    //list of gameobjects robot joints arms
    //public List<GameObject> robotJointsArms = new List<GameObject>();
    
    //struct containing a string and a gameobject
    [System.Serializable]
    public class RobotJointsArmsDict
    {
        //list of joints
        public GameObject joint;
        public String cwKey;
        public String ccKey;
        public String axis;
        public String direction;
        public bool invert;
        public int range = 180;
    }

    //list of RobotJointsArmsDict
    public List<RobotJointsArmsDict> _robotJointsArmsDict = new List<RobotJointsArmsDict>();
    //list of strings
    public List<KeyCode> _keys = new List<KeyCode>();
    //button to fire updatekeys
    public bool _updateKeysButton = false;

    //public first person camera gameobject
    public Camera _firstPersonCamera;
    //public third person camera gameobject
    public Camera _thirdPersonCamera;
    public RawImage _renderPlane;
    Gamepad _gamepad;
    
    //public enum containing keyboard and joystick selectable inputs
    public enum InputType
    {
        Keyboard,
        Joystick
    }

    public enum JoystickMode
    {
        Hold,
        Move
    }

    //public InputType
    public InputType _inputType = InputType.Joystick;
    public JoystickMode _joystickMode = JoystickMode.Hold;
    //controlsSO list
    public List<ControlsSO> _controlsList = new List<ControlsSO>();
    private ControlsSO _activeControls = null;
    public InputSettings _inputSettings;
    //set global input to input settings
    //private controller
    private CharacterController controller;
    void Awake()
    {
        //set global input to input settings
        InputSystem.settings = _inputSettings;
    }
    public Transform[] _rigidBodiesInitialTransforms;
    public Transform[] _rigidBodiesTransforms;
    // Start is called before the first frame update
    void Start()
    {
        //copy _rigidBodiesTransforms into _rigidBodiesInitialTransforms, they are already filled
        _rigidBodiesInitialTransforms = new Transform[_rigidBodiesTransforms.Length];
        for (int i = 0; i < _rigidBodiesTransforms.Length; i++)
        {
            //store a copy
            //new
            _rigidBodiesInitialTransforms[i] = new GameObject().transform;
            //copy
            _rigidBodiesInitialTransforms[i].position = _rigidBodiesTransforms[i].position;
            _rigidBodiesInitialTransforms[i].rotation = _rigidBodiesTransforms[i].rotation;

        }
        
        //init controller
        controller = GetComponent<CharacterController>();
        _gamepad = Gamepad.current;
        /*if (_gamepad == null)
        {
            Debug.LogWarning("Logitech F710 not detected or not supported.");
            return;
        }*/
        if (_inputType == InputType.Keyboard)
        {
            //get active controls
            _activeControls = _controlsList[0];
            UpdateKeysKeyboard();
        }
        else if (_inputType == InputType.Joystick)
        {
            //get active controls
            _activeControls = _controlsList[1];
            UpdateKeysJoystick();
        }
        //_thirdPersonCamera.enabled = false;

        //insert R key and gameobject in dictionary
        //robotJointsArmsDict.Add("R", GameObject.Find("Cube (4)"));
        //render third person camera into _renderPlane raw image
        //_renderPlane.GetComponent<RawImage>().texture = _thirdPersonCamera.targetTexture;

    }

    private void ResetRigidBodies()
    {  
        //log RESET BODIES
        Debug.Log("RESET BODIES");
        //reset rigid bodies
        for (int i = 0; i < _rigidBodiesInitialTransforms.Length; i++)
        {
            _rigidBodiesTransforms[i].transform.position = _rigidBodiesInitialTransforms[i].position;
            _rigidBodiesTransforms[i].transform.rotation = _rigidBodiesInitialTransforms[i].rotation;
        }
    }

    void FixedUpdate(){
        //charactercontroller move down
        controller.Move(transform.up * -0.1f);
        //if P is pressed reset rigid bodies
        if (Input.GetKeyDown(KeyCode.P))
        {
            //reset rigid bodies
            ResetRigidBodies();
        }
        if (_inputType == InputType.Keyboard)
        {
            KeyboardUpdate();
        }
        else if (_inputType == InputType.Joystick)
        {
            if(_joystickMode == JoystickMode.Hold)
                try{
                    JoystickUpdateHold();
                }
                catch (Exception e){
                    Debug.Log("No gamepad");
                }
            else if (_joystickMode == JoystickMode.Move)
                try{
                    JoystickUpdateMove();
                }
                catch (Exception e){
                    Debug.Log("No gamepad");
                }
        }
        //update camera
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (_firstPersonCamera.enabled)
            {
                //set tag of first person camera to untagged
                _firstPersonCamera.tag = "Untagged";
                //set tag of third person camera to main camera
                _thirdPersonCamera.tag = "MainCamera";
                //switch render plane to first person, render is a raw image
            }
            else
            {
                //set tag of third person camera to untagged
                _thirdPersonCamera.tag = "Untagged";
                //set tag of first person camera to main camera
                _firstPersonCamera.tag = "MainCamera";
                //switch render plane to third person
                
            }
        }
        //if left or right arrows are pressed rotate third person camera around robot
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            _thirdPersonCamera.transform.RotateAround(transform.position, Vector3.up, 1f);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            _thirdPersonCamera.transform.RotateAround(transform.position, Vector3.up, -1f);
        }
    }

    void KeyboardUpdate(){
        if (Input.anyKey)
        {
            // Loop through all the possible key codes and check if the key is pressed
            foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKey(keyCode))
                {
                    print(keyCode);
                    //if key pressed is in list of keys, check each occurrence
                    if(_keys.Contains(keyCode))
                    {
                        //get list of indexes of key pressed
                        List<int> indexes = new List<int>();
                        for (int i = 0; i < _keys.Count; i++)
                        {
                            if (_keys[i] == keyCode)
                            {
                                indexes.Add(i);
                            }
                        }
                        //for each index do
                        foreach (int index in indexes)
                        {
                            //if index is associated with gameobject Odile
                            if (_robotJointsArmsDict[index / 2].joint.name.Contains(gameObject.name))
                            {
                                //if direction is MoveStrafe
                                if (_robotJointsArmsDict[index / 2].direction == "MoveStrafe")
                                {
                                    //move odile right
                                    controller.Move(transform.right * (index % 2 == 0 ? 1 : -1) / _moveUpdate);
                                }
                                //if direction is MoveForward
                                if (_robotJointsArmsDict[index / 2].direction == "MoveForward")
                                {
                                    //move odile forward
                                    controller.Move(transform.forward * (index % 2 == 0 ? 1 : -1) / _moveUpdate);
                                }
                                //if direction is MoveRotate
                                if (_robotJointsArmsDict[index / 2].direction == "MoveRotateRight")
                                {
                                    //rotate odile right
                                    controller.transform.eulerAngles += new Vector3(0, (index % 2 == 0 ? 1 : -1) / _angleUpdate, 0);
                                }
                            }
                            else{
                                //if index is even
                                if (index % 2 == 0)
                                {
                                    //store parent
                                    Transform father = _robotJointsArmsDict[index / 2].joint.transform.parent;
                                    //rotate joint clockwise around father
                                    _robotJointsArmsDict[index / 2].joint.transform.RotateAround(father.position, father.right, 1f);
                                }
                                //if index is odd
                                else
                                {
                                    //rotate joint counterclockwise
                                    Transform father = _robotJointsArmsDict[(index - 1) / 2].joint.transform.parent;
                                    _robotJointsArmsDict[(index - 1) / 2].joint.transform.RotateAround(father.position, father.right, -1f);
                                }
                            }
                        }
                    }
                }
            }
        }
        if (_updateKeysButton)
        {
            UpdateKeysKeyboard();
            _updateKeysButton = false;
        }
    }

    //prevaxisvalue dictionary
    //TODO FIX JOINTS THAT ARE REPEATED ON MORE AXES
    //prev value is not just for joint but also for axis
    Dictionary<string, float> _prevJointValue = new Dictionary<string, float>();
    Dictionary<string, string> _prevAxisValue = new Dictionary<string, string>();
    //dictionary of 2 string tuples and float
    Dictionary<Tuple<string, string>, float> _prevAxisValue2 = new Dictionary<Tuple<string, string>, float>();
    public float  _moveUpdate = 100f;
    public float  _angleUpdate = 10f;
    void JoystickUpdateHold(){
        //print all the buttons that were pressed
        foreach (InputControl control in _gamepad.allControls)
        {
            //print control.displayName if value > 0
            /*if (control.ReadValueAsObject().ToString() != "0")
            {
                Debug.Log(control.displayName + " " + control.ReadValueAsObject().ToString());
            }*/
            /*
            Right Bumper
            D-Pad Y
            X
            Y
            ...
            */
            //find all occurrencies
            List<RobotJointsArmsDict> occurrencies = _robotJointsArmsDict.FindAll(x => x.axis == control.displayName);
            if(occurrencies.Count > 0){
                //log occurrencies if count > 1
                /*if (occurrencies.Count > 1)
                {
                    Debug.Log("More than one occurrency of " + control.displayName);
                    foreach (RobotJointsArmsDict occurrency in occurrencies)
                    {
                        Debug.Log(occurrency.joint.name);
                    }
                }*/
                //do the same but for all occurrencies
                foreach (RobotJointsArmsDict occurrency in occurrencies)
                {
                    //if occurrency joint is null continue
                    if (occurrency.joint == null)
                    {
                        continue;
                    }
                    string jointAxis = occurrency.joint.name + " " + occurrency.axis;
                    //if occurrency name contains Odile controls move it
                    if(occurrency.joint.name.Contains(gameObject.name)){
                        ////if dpad y
                        //if (control.displayName == "D-Pad Y")
                        //{
                        //    //move odile up and forward to transform forward
                        //    //occurrency.joint.transform.position += float.Parse(control.ReadValueAsObject().ToString()) * occurrency.joint.transform.forward / _moveUpdate;
                        //    //controller move
                        //    controller.Move(transform.forward * float.Parse(control.ReadValueAsObject().ToString()) / _moveUpdate);
                        //}
                        //if (control.displayName == "D-Pad X")
                        //{
                        //    //move odile up and forward to transform forward
                        //    //occurrency.joint.transform.position += float.Parse(control.ReadValueAsObject().ToString()) * occurrency.joint.transform.right / _moveUpdate;
                        //    controller.Move(transform.right * float.Parse(control.ReadValueAsObject().ToString()) / _moveUpdate);
                        //}
                        //if (control.displayName == "Right Bumper")
                        //{
                        //    //rotate odile right, sum angle to euler rotation
                        //    //occurrency.joint.transform.eulerAngles += new Vector3(0, float.Parse(control.ReadValueAsObject().ToString())/_angleUpdate, 0);
                        //    controller.transform.eulerAngles += new Vector3(0, float.Parse(control.ReadValueAsObject().ToString())/_angleUpdate, 0);
                        //}
                        //if (control.displayName == "Left Bumper")
                        //{
                        //    //rotate odile right, sum angle to euler rotation
                        //    //occurrency.joint.transform.eulerAngles += new Vector3(0, -float.Parse(control.ReadValueAsObject().ToString())/_angleUpdate, 0);
                        //    controller.transform.eulerAngles += new Vector3(0, -float.Parse(control.ReadValueAsObject().ToString())/_angleUpdate, 0);
                        //}
                        //if occurrency.joint.name == StrafeLeft
                        if (occurrency.direction == "MoveStrafe")
                        {
                            controller.Move(transform.right * float.Parse(control.ReadValueAsObject().ToString()) / _moveUpdate);
                        }
                        //if occurrency.joint.name == Forward
                        if (occurrency.direction == "MoveForward")
                        {
                            controller.Move(transform.forward * float.Parse(control.ReadValueAsObject().ToString()) / _moveUpdate);
                        }
                        //if occurrency.joint.name == RotateLeft
                        if (occurrency.direction == "MoveRotateRight")
                        {
                            controller.transform.eulerAngles += new Vector3(0, float.Parse(control.ReadValueAsObject().ToString()) / _angleUpdate, 0);
                        }
                        //if occurrency.joint.name == RotateRight
                        if (occurrency.direction == "MoveRotateLeft")
                        {
                            controller.transform.eulerAngles += new Vector3(0, -float.Parse(control.ReadValueAsObject().ToString()) / _angleUpdate, 0);
                        }
                    }
                    else{
                        //update prevaxisvalue dictionary
                        if (!_prevJointValue.ContainsKey(jointAxis))
                        {
                            _prevJointValue.Add(jointAxis, 0);
                        }
                        string value = control.ReadValueAsObject().ToString();
                        //value to float
                        float valueFloat = float.Parse(value);
                        //Debug.Log("Axis " + control.displayName + " " + valueFloat);
                        //store parent
                        Transform father = occurrency.joint.transform.parent;
                        Transform child = occurrency.joint.transform;
                        //save distance amount between joint and father
                        //float distance = Vector3.Distance(father.position, child.position);
                        //move joint to father
                        //child.position = father.position;
                        //if invert is true
                        int invert = 1;
                        if (occurrency.invert)
                        {
                            //invert value
                            invert = -1;
                        }
                        //rotate back by prevvalue
                        ////child.RotateAround(father.position, father.right, -invert * float.Parse(_prevAxisValue[occurrency.joint.name]) * 90);
                        //map value to 0 180 and rotate joint to that precise angle relative to father's right
                        ////child.RotateAround(father.position, father.right, invert * valueFloat * 90);
                        //child.RotateAround(father.position, father.right, invert * (valueFloat - _prevJointValue[jointAxis]) * occurrency.range / 2);
                        father.Rotate(Vector3.right, invert * (valueFloat - _prevJointValue[jointAxis]) * occurrency.range / 2);
                        //get vector pointing as child rotation
                        //Vector3 vector = child.rotation * Vector3.up;
                        //move joint to distance amount from father
                        //child.position += vector * distance;
                        //set prevaxisvalue to value
                        _prevJointValue[jointAxis] = valueFloat;
                    }
                }
            }
        }
        if (_updateKeysButton)
        {
            UpdateKeysJoystick();
            _updateKeysButton = false;
        }
    }

    void JoystickUpdateMove(){
        //print all the buttons that were pressed
        foreach (InputControl control in _gamepad.allControls)
        {
            List<RobotJointsArmsDict> occurrencies = _robotJointsArmsDict.FindAll(x => x.axis == control.displayName);

            if(occurrencies.Count > 0){
                //do the same but for all occurrencies
                foreach (RobotJointsArmsDict occurrency in occurrencies)
                {
                    //if occurrency joint is null continue
                    if (occurrency.joint == null)
                    {
                        continue;
                    }
                    if(occurrency.joint.name.Contains(gameObject.name)){
                        if (occurrency.direction == "MoveStrafe")
                        {
                            controller.Move(transform.right * float.Parse(control.ReadValueAsObject().ToString()) / _moveUpdate);
                        }
                        //if occurrency.joint.name == Forward
                        if (occurrency.direction == "MoveForward")
                        {
                            controller.Move(transform.forward * float.Parse(control.ReadValueAsObject().ToString()) / _moveUpdate);
                        }
                        //if occurrency.joint.name == RotateLeft
                        if (occurrency.direction == "MoveRotate")
                        {
                            controller.transform.eulerAngles += new Vector3(0, float.Parse(control.ReadValueAsObject().ToString()) / _angleUpdate, 0);
                        }
                    }
                    else{
                        //update prevaxisvalue dictionary
                        if (!_prevJointValue.ContainsKey(occurrency.joint.name))
                        {
                            _prevJointValue.Add(occurrency.joint.name, 0);
                        }
                        string value = control.ReadValueAsObject().ToString();

                        //value to float
                        float valueFloat = float.Parse(value);
                        //Debug.Log("Axis " + control.displayName + " " + valueFloat);
                        //store parent
                        Transform father = occurrency.joint.transform.parent;
                        Transform child = occurrency.joint.transform;
                        //save distance amount between joint and father
                        //float distance = Vector3.Distance(father.position, child.position);
                        //move joint to father
                        //child.position = father.position;
                        //if invert is true
                        int invert = 1;
                        if (occurrency.invert)
                        {
                            //invert value
                            invert = -1;
                        }
                        //rotate back by prevvalue
                        //child.RotateAround(father.position, father.right, -invert * float.Parse(_prevAxisValue[occurrency.joint.name]) * 90);
                        //map value to 0 180 and rotate joint to that precise angle relative to father's right
                        //child.RotateAround(father.position, father.right, invert * valueFloat);
                        father.Rotate(Vector3.right, invert * valueFloat);
                        //get vector pointing as child rotation
                        //Vector3 vector = child.rotation * Vector3.up;
                        //move joint to distance amount from father
                        //child.position += vector * distance;
                        //set prevaxisvalue to value
                        _prevJointValue[occurrency.joint.name] = valueFloat;
                    }
                }
            }
        }
        if (_updateKeysButton)
        {
            UpdateKeysJoystick();
            _updateKeysButton = false;
        }
    }
    void UpdateKeysKeyboard(){
        //clear list
        _keys.Clear();
        //clear _robotJointsArmsDict
        _robotJointsArmsDict.Clear();
        //for each element in active controls fill _robotJointsArmsDict using gameobject find
        foreach (ControlsSO.RobotJointsArmsDict robotJointsArm in _activeControls._robotJointsArmsDict)
        {
            //add key to list parsing string, case insensitive
            _robotJointsArmsDict.Add(new RobotJointsArmsDict{joint = GameObject.Find(robotJointsArm.joint), cwKey = robotJointsArm.cwKey, ccKey = robotJointsArm.ccKey, axis = robotJointsArm.axis, direction = robotJointsArm.direction});
        }
        //populate list of keys using RobotJointsArmsDict
        foreach (RobotJointsArmsDict robotJointsArm in _robotJointsArmsDict)
        {
            //add key to list parsing string, case insensitive
            _keys.Add((KeyCode)System.Enum.Parse(typeof(KeyCode), robotJointsArm.cwKey, true));
            _keys.Add((KeyCode)System.Enum.Parse(typeof(KeyCode), robotJointsArm.ccKey, true));
        }
        //log keys updated
        Debug.Log("Keys updated");
    }

    void UpdateKeysJoystick(){
        //clear list
        _keys.Clear();
        //clear _robotJointsArmsDict
        _robotJointsArmsDict.Clear();
        //for each element in active controls fill _robotJointsArmsDict using gameobject find
        foreach (ControlsSO.RobotJointsArmsDict robotJointsArm in _activeControls._robotJointsArmsDict)
        {
            //add key to list parsing string, case insensitive
            _robotJointsArmsDict.Add(new RobotJointsArmsDict{joint = GameObject.Find(robotJointsArm.joint), axis = robotJointsArm.axis, direction = robotJointsArm.direction});
            //invert
            _robotJointsArmsDict[_robotJointsArmsDict.Count - 1].invert = robotJointsArm.invert;
        }
        //populate list of keys using RobotJointsArmsDict
        foreach (RobotJointsArmsDict robotJointsArm in _robotJointsArmsDict)
        {
            //add key to list parsing string, case insensitive, if not null
            if (robotJointsArm.cwKey != null)
            {
                _keys.Add((KeyCode)System.Enum.Parse(typeof(KeyCode), robotJointsArm.cwKey, true));
            }
            if (robotJointsArm.ccKey != null)
            {
                _keys.Add((KeyCode)System.Enum.Parse(typeof(KeyCode), robotJointsArm.ccKey, true));
            }
        }
    }
    private string axisInput = ""; // Variable to store the user input
    private string jointInput = ""; // Variable to store the user input
    private int numButtons = 20; // Number of buttons
    private float buttonHeight = 30f; // Height of each button
    private float spacing = 10f; // Spacing between buttons
    private Vector2 scrollPosition = Vector2.zero; // Scroll position
    public List<int> selectedOption = new List<int>(); // List of selected options
    public List<int> prevSelectedOption = new List<int>(); // List of selected options
    public List<bool> showOptions = new List<bool>(); // List of booleans to show/hide the options
    private bool listSetup = false; // Boolean to check if the list has been setup
    public string[] options = { "Right Stick X", "Right Stick Y", "Left Stick X", "Left Stick Y", "Right Trigger", "Left Trigger", "D-Pad X", "D-Pad Y", "Right Bumper", "Left Bumper", "A", "B", "X", "Y", "Start", "Select", "Left Stick Button", "Right Stick Button", "Left Stick Button", "Right Stick Button", "None" };
    private bool _hideGui = false;
    private void OnGUI()
    {
        if (!_guiEnabled)
            return;
        GUIStyle customStyle = new GUIStyle(GUI.skin.box);
        // If the list has not been setup, set it up
        if (!listSetup)
        {
            //add one bool for each element in _robotJointsArmsDict
            for (int i = 0; i < _robotJointsArmsDict.Count; i++)
            {
                showOptions.Add(false);
                selectedOption.Add(0);
                prevSelectedOption.Add(0);
                //for each selected option set index to name of corresponding robot joint, if not present set to last
                selectedOption[i] = Array.IndexOf(options, _robotJointsArmsDict[i].axis);
                if (selectedOption[i] == -1)
                {
                    selectedOption[i] = options.Length - 1;
                }
                prevSelectedOption[i] = selectedOption[i];
            }
            listSetup = true;
        }
        float scrollViewHeight = (numButtons * buttonHeight) + ((numButtons - 1) * spacing);
        float scrollViewWidth = 500f;
        //button to hide or show gui
        if (GUILayout.Button("Hide/Show Control GUI"))
        {
            _hideGui = !_hideGui;
        }
        if (_hideGui)
        {
            return;
        }

        // Begin the scroll view
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, customStyle, GUILayout.Width(scrollViewWidth), GUILayout.Height(600));
        //set background
        GUI.backgroundColor = Color.grey;
        //button to switch keyboard and joystick
        GUILayout.Label("Robot controls editor");
        if (GUILayout.Button("Switch input type: " + _inputType, customStyle))
        {
            if (_inputType == InputType.Keyboard)
            {
                _inputType = InputType.Joystick;
                //get active controls
                _activeControls = _controlsList[1];
                UpdateKeysJoystick();
            }
            else if (_inputType == InputType.Joystick)
            {
                _inputType = InputType.Keyboard;
                //get active controls
                _activeControls = _controlsList[0];
                UpdateKeysKeyboard();
            }
        }
        InputControl controlPressed = null;
        if (_inputType == InputType.Keyboard)
        {
            GUILayout.Label("WARNING: GUI does not work for keyboard");
        }
        else if (_inputType == InputType.Joystick){
            //get gamepad pressed command and if it matches with one options
            try{
            foreach (InputControl control in _gamepad.allControls)
                {
                    //if control is not zero and it is in options store it in controlPressed and break
                    if (control.ReadValueAsObject().ToString() != "0" && Array.IndexOf(options, control.displayName) != -1)
                    {
                        controlPressed = control;
                        break;
                    }
                }
                GUILayout.Label("GAMEPAD OK");
            }
            catch (Exception e)
            {
                //add NO GAMEPAD text
                GUILayout.Label("!!!NO GAMEPAD DETECTED!!!");
            }
        }
        
        GUILayout.Label("Active controls: " + _activeControls.name);
        if (GUILayout.Button("Mode " + _joystickMode, customStyle)){
            if (_joystickMode == JoystickMode.Hold)
                _joystickMode = JoystickMode.Move;
            else if (_joystickMode == JoystickMode.Move)
                _joystickMode = JoystickMode.Hold;
        }
        GUILayout.Label("Press arrows to rotate third person camera");
        //press p to reset solids
        GUILayout.Label("Press P to reset solids");

        // Text input field to allow the user to change the value
        //GUILayout.Label("Enter new axis value:");
        //axisInput = GUILayout.TextField(axisInput, 25); // '25' is the maximum character limit (optional)
        //GUILayout.Label("Enter new Joint value:");
        //jointInput = GUILayout.TextField(jointInput, 25); // '25' is the maximum character limit (optional)
        // Only show the options if the showOptions variable is true
        
        //horizontal two buttons, save and load _robotJointsArmsDict to file using json
        /*GUILayout.BeginHorizontal();
        //label save button maybe works, will save in game folder
        GUILayout.Label("Save button maybe works, will save in game folder if it does");
        if (GUILayout.Button("Save"))
        {
            //store _robotJointsArmsDict into a string, write manually
            string dict;
            dict = "[";
            //for each field in _robotJointsArmsDict write it
            foreach (RobotJointsArmsDict robotJointsArm in _robotJointsArmsDict)
            {
                dict += "{\"joint\":\"" + robotJointsArm.joint + "\",\"axis\":\"" + robotJointsArm.axis + "\"},";
            }
            dict = dict.Remove(dict.Length - 1);
            dict += "]";

            //create file
            System.IO.File.WriteAllText(Application.dataPath + "/robotJointsArmsDict.json", dict);
        }
        if (GUILayout.Button("Load"))
        {
            _updateKeysButton = true;
        }
        GUILayout.EndHorizontal();*/

        GUILayout.Label("Keys: ");

        // Loop through the dictionary elements
        try{
            foreach (RobotJointsArmsDict robotJointsArm in _robotJointsArmsDict)
            {
                //if robotJointsArm joint is null continue
                if (robotJointsArm.joint == null)
                {
                    continue;
                }
                int robotJointsArmsIndex = _robotJointsArmsDict.IndexOf(robotJointsArm);
                GUILayout.BeginHorizontal();
                // Display the joint name and axis
                GUILayout.Label(robotJointsArm.joint.name + robotJointsArm.direction + " " + robotJointsArm.axis);

                // Add a button to modify the value for this dictionary entry
                /*if (GUILayout.Button("Change"))
                {
                    // Example: If you want to update the axis value of this entry
                    robotJointsArm.axis = axisInput;
                    //robotJointsArm.joint = GameObject.Find(jointInput);
                }*/
                if (GUILayout.Button(options[selectedOption[robotJointsArmsIndex]]))
                {
                    // Toggle the visibility of the options when the main button is clicked
                    showOptions[robotJointsArmsIndex] = !showOptions[robotJointsArmsIndex];
                    //set all other actives to false
                    for (int i = 0; i < showOptions.Count; i++)
                    {
                        if (i != robotJointsArmsIndex)
                        {
                            showOptions[i] = false;
                        }
                    }
                }
                
                if (showOptions[robotJointsArmsIndex] == true)
                {
                    // Display the options as a toggle group
                    selectedOption[robotJointsArmsIndex] = GUILayout.SelectionGrid(selectedOption[robotJointsArmsIndex], options, 1);
                    robotJointsArm.axis = options[selectedOption[robotJointsArmsIndex]];
                    //if an option is clicked close the options
                    if (selectedOption[robotJointsArmsIndex] != prevSelectedOption[robotJointsArmsIndex])
                    {
                        showOptions[robotJointsArmsIndex] = false;
                        prevSelectedOption[robotJointsArmsIndex] = selectedOption[robotJointsArmsIndex];
                    }
                    //if controlPressed is not null close the options
                    if (controlPressed != null)
                    {
                        showOptions[robotJointsArmsIndex] = false;
                        //find control in options and set its index to selectedOption
                        selectedOption[robotJointsArmsIndex] = Array.IndexOf(options, controlPressed.displayName);
                        prevSelectedOption[robotJointsArmsIndex] = selectedOption[robotJointsArmsIndex];
                    }
                }
                // button to toggle invert
                if (GUILayout.Button("Invert " + (robotJointsArm.invert? "True" : "False")))
                {
                    // Example: If you want to update the axis value of this entry
                    robotJointsArm.invert = !robotJointsArm.invert;
                }
                // short range text input
                GUILayout.Label("Range: ");
                robotJointsArm.range = int.Parse(GUILayout.TextField(robotJointsArm.range.ToString(), 3));

                if(!robotJointsArm.joint.name.Contains(gameObject.name)){
                    if (GUILayout.Button("Copy"))
                    {
                        // Example: Add a new entry to the dictionary with default values
                        RobotJointsArmsDict newEntry = new RobotJointsArmsDict();
                        newEntry.joint = robotJointsArm.joint;
                        newEntry.axis = axisInput;
                        /*
                        _robotJointsArmsDict.Add(newEntry);
                        showOptions.Add(false);
                        selectedOption.Add(0);
                        prevSelectedOption.Add(0);
                        */
                        //add everything at this position
                        _robotJointsArmsDict.Insert(robotJointsArmsIndex, newEntry);
                        showOptions.Insert(robotJointsArmsIndex, false);
                        selectedOption.Insert(robotJointsArmsIndex, 0);
                        prevSelectedOption.Insert(robotJointsArmsIndex, 0);
                        //update all other indexes
                        for (int i = robotJointsArmsIndex + 1; i < showOptions.Count; i++)
                        {
                            selectedOption[i] = Array.IndexOf(options, _robotJointsArmsDict[i].axis);
                            if (selectedOption[i] == -1)
                            {
                                selectedOption[i] = options.Length - 1;
                            }
                            prevSelectedOption[i] = selectedOption[i];
                        }
                    }
                    if (GUILayout.Button("Remove"))
                    {
                        //if there are no other entries with this name do not remove it
                        if (_robotJointsArmsDict.FindAll(x => x.joint == robotJointsArm.joint).Count > 1)
                        {
                            // Example: Add a new entry to the dictionary with default values
                            _robotJointsArmsDict.Remove(robotJointsArm);
                            showOptions.RemoveAt(robotJointsArmsIndex);
                            selectedOption.RemoveAt(robotJointsArmsIndex);
                            prevSelectedOption.RemoveAt(robotJointsArmsIndex);
                            //update all other indexes
                            for (int i = robotJointsArmsIndex; i < showOptions.Count; i++)
                            {
                                selectedOption[i] = Array.IndexOf(options, _robotJointsArmsDict[i].axis);
                                if (selectedOption[i] == -1)
                                {
                                    selectedOption[i] = options.Length - 1;
                                }
                                prevSelectedOption[i] = selectedOption[i];
                            }
                        }
                    }
                }

                GUILayout.EndHorizontal();
            }
        /*
        // Add button to add new dictionary entry
        if (GUILayout.Button("Add New Entry"))
        {
            // Example: Add a new entry to the dictionary with default values
            RobotJointsArmsDict newEntry = new RobotJointsArmsDict();
            newEntry.axis = userInput;
            _robotJointsArmsDict.Add(newEntry);
        }*/
    
        GUILayout.EndScrollView();
        }
        //catch foreach exception
        catch(InvalidOperationException e){
            Debug.Log(e);
        }
    }
}
