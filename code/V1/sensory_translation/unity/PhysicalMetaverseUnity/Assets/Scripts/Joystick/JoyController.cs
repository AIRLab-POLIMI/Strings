using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class JoyController : MonoBehaviour
{
    
    #region Variables

        [SerializeField] private string key;
    
        // one SensorValue for each rotation axis  
        private SensorValue _ax;
        private SensorValue _ay;
        private SensorValue _az;
        
        // ones sensor value for each of the two inputs of the joypad
        private SensorValue _jx;
        private SensorValue _jy;
        
        // one sensor value for each of the two buttons of the joypad
        private SensorValue _btrig;
        private SensorValue _bgrab;
        
    #endregion
    
    
    #region Methods

        void Start()
        {
            // initialize the sensor values
            _ax = new SensorValue("ax", -180, 180);
            _ay = new SensorValue("ay", -180, 180);
            _az = new SensorValue("az", -180, 180);
            _jx = new SensorValue("jx", -1, 1);
            _jy = new SensorValue("jy", -1, 1);
            _btrig = new SensorValue("bt", 0, 1);
            _bgrab = new SensorValue("bg", 0, 1);
        }

        // function that is a callback when an input from joypad changes
        public void OnJoyPadChanged(InputAction.CallbackContext context)
        {
            var val = context.ReadValue<Vector2>();
            _ax.OnNewValueReceived(val.x);
            _ay.OnNewValueReceived(val.y);
        }

        public void OnTrigBtnChanged(InputAction.CallbackContext context)
        {
            var val = context.ReadValue<float>();
            _btrig.OnNewValueReceived(val);
        }
        
        public void OnGrabBtnChanged(InputAction.CallbackContext context)
        {
            var val = context.ReadValue<float>();
            _bgrab.OnNewValueReceived(val);
        }
        
        // Update is called once per frame
        public string TryGetMsg()
        {
            // get current transform rotation
            var curRot = transform.rotation.eulerAngles;
            
            // set the current rotation values in the sensor values
            _ax.OnNewValueReceived(curRot.x);
            _ay.OnNewValueReceived(curRot.y);
            _az.OnNewValueReceived(curRot.z);
            
            // generate empty message
            var msg = "";

            // try to get the message from each sensor value
            AddMsg(_ax, msg);
            AddMsg(_ay, msg);
            AddMsg(_az, msg);
            AddMsg(_jx, msg);
            AddMsg(_jy, msg);
            AddMsg(_btrig, msg);
            AddMsg(_bgrab, msg);

            // send the current message
            return msg != "" ? $"{key}:{msg}" : "";
        }

        private string AddMsg(SensorValue sensorValue, string currentMsg)
        {
            string sensorMsg = sensorValue.TryGetMsg();
            if (sensorMsg != "") 
                currentMsg += ("_" + sensorMsg);
            return currentMsg;
        }
            
    #endregion

}
