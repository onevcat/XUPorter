using UnityEngine;
using System.Collections;


namespace UnityEditor.XCodeEditor{


//	com.apple.ApplePay 
//	com.apple.ApplicationGroups.iOS
//	com.apple.BackgroundModes  
//	com.apple.DataProtection 
//	com.apple.GameCenter
//	com.apple.HealthKit 
//	com.apple.HomeKit 
//	com.apple.InAppPurchase 
//	com.apple.InterAppAudio
//	com.apple.Keychain 
//	com.apple.Maps.iOS
//	com.apple.Push 
//	com.apple.SafariKeychain
//	com.apple.Siri 
//	com.apple.VPNLite 
//	com.apple.WAC 
//	com.apple.Wallet 
//	com.apple.iCloud


	public enum XCProjectSystemCapabilitiesType
	{
		XCProjectSystemCapabilitiesTypeApplePay,
		XCProjectSystemCapabilitiesTypeApplePush,
		XCProjectSystemCapabilitiesTypeAppleiCloud

	};

	public class XCProjectSystemCapabilities  {


		public static XCProjectSystemCapabilities createWithProject(PBXProject project){

			XCProjectSystemCapabilities systemCapabilities = new XCProjectSystemCapabilities (); 
			systemCapabilities.weakProject = project;
			return systemCapabilities;
		}
		public static string getEnumType(XCProjectSystemCapabilitiesType type){

			string result=null;
			switch (type) {
			case XCProjectSystemCapabilitiesType.XCProjectSystemCapabilitiesTypeApplePay:
				result="com.apple.ApplePay";
				break;
			case XCProjectSystemCapabilitiesType.XCProjectSystemCapabilitiesTypeApplePush:
				result = "com.apple.Push";
				break;
			case XCProjectSystemCapabilitiesType.XCProjectSystemCapabilitiesTypeAppleiCloud:
				result = "com.apple.iCloud";
				break;
			}
			return result;
		}

		public PBXProject weakProject;

		public void visitAddSystemCapabilities (XCProjectSystemCapabilitiesType type, bool enabled){

			if (weakProject == null) {
				Debug.Log ("weakProject must not be null");
				return;
			}
			string destributeType = getEnumType (type);
			Debug.Log ("Add System Capabilities "+destributeType);

			PBXDictionary _Attributes = (PBXDictionary)weakProject.data ["attributes"];
			PBXDictionary _TargetAttributes = (PBXDictionary)_Attributes ["TargetAttributes"];
			PBXList _targets = (PBXList)weakProject.data ["targets"];
			PBXDictionary targetDict = null;
			if (_TargetAttributes.ContainsKey ((string)_targets [0])) {
				targetDict = (PBXDictionary)_TargetAttributes [(string)_targets [0]];
			} else {
				//不会发生
				//return;
				targetDict = new PBXDictionary();
			}
//			Debug.Log ("targetDict:" + targetDict);

			PBXDictionary SystemCapabilities = null;
			if (targetDict != null && targetDict.ContainsKey ("SystemCapabilities")) {
//				Debug.Log ("xxxxxxxxxxxxxxxxxxx");
				SystemCapabilities = (PBXDictionary)targetDict ["SystemCapabilities"];
			} else {
				SystemCapabilities = new PBXDictionary();
			}

			Debug.Log ("before SystemCapabilities:" + SystemCapabilities);
			if (SystemCapabilities!=null && SystemCapabilities.ContainsKey (destributeType)) {
				SystemCapabilities.Remove (destributeType);
			}
			Debug.Log ("after SystemCapabilities:" + SystemCapabilities);
			PBXDictionary enableDict = new PBXDictionary ();
			enableDict.Add ("enabled", enabled?"1":"0");
			SystemCapabilities.Add (destributeType, enableDict);

			if (!targetDict.ContainsKey ("SystemCapabilities")) {
				targetDict.Add("SystemCapabilities",SystemCapabilities);
			}
			if (!_TargetAttributes.ContainsKey ((string)_targets [0])) {
				_TargetAttributes.Add((string)_targets [0],targetDict);
			}

		}
	}

}
