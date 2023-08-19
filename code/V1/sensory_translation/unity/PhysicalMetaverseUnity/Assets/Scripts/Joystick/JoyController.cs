using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class JoyController : MonoBehaviour
{
    
    #region Variables

        [SerializeField] private string key;
    
        [Range(0, 180)]
        [SerializeField] float accXRange = 180;
        [Range(0, 180)]
        [SerializeField] float accYRange = 180;
        [Range(0, 180)]
        [SerializeField] float accZRange = 180;
        
        // one SensorValue for each rotation axis  
        private SensorValue _rx;
        private SensorValue _ry;
        private SensorValue _rz;
        
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
            _rx = new SensorValue("rx", -accXRange, accXRange);
            _ry = new SensorValue("ry", -accYRange, accYRange);
            _rz = new SensorValue("rz", -accZRange, accZRange);
            _jx = new SensorValue("jx", -1, 1);
            _jy = new SensorValue("jy", -1, 1);
            _btrig = new SensorValue("bt", 0, 1);
            _bgrab = new SensorValue("bg", 0, 1);
        }

        // function that is a callback when an input from joypad changes
        public void OnJoyPadChanged(InputAction.CallbackContext context)
        {
            var val = context.ReadValue<Vector2>();
            _jx.OnNewValueReceived(val.x);
            _jy.OnNewValueReceived(val.y);
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
            _rx.OnNewValueReceived(curRot.x);
            _ry.OnNewValueReceived(curRot.y);
            _rz.OnNewValueReceived(curRot.z);
            
            // print in the debug log the current value of _rx, _ry, _rz
            // Debug.Log($"rx: {_rx.CurrentValue} ry: {_ry.CurrentValue} rz: {_rz.CurrentValue}");
            
            // generate empty message
            var msg = "";

            // try to get the message from each sensor value
            msg = AddMsg(_rx, msg);
            msg = AddMsg(_ry, msg);
            msg = AddMsg(_rz, msg);
            msg = AddMsg(_jx, msg);
            msg = AddMsg(_jy, msg);
            msg = AddMsg(_btrig, msg);
            msg = AddMsg(_bgrab, msg);
            
            // send the current message
            return msg;
        }

        private string AddMsg(SensorValue sensorValue, string currentMsg)
        {
            string sensorMsg = sensorValue.TryGetMsg();

            if (sensorMsg != "")
            {
                // add "key" to the message at the beginning
                sensorMsg = key + sensorMsg;

                if (currentMsg == "")
                    currentMsg = sensorMsg;
                else
                    currentMsg += ("_" + sensorMsg);
            }
            
            return currentMsg;
        }
            
    #endregion

}
