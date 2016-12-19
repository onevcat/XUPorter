using UnityEngine;
using System.Collections;

using System.Collections.Generic;

namespace PlistCS{

	public class XCPlistDocument {

		//私有变量
		private string pathForPlist=null;
		private Dictionary<string, object> root=null;

		//初始化的类方法
		public static XCPlistDocument createWithPlistPath (string path){
			if (string.IsNullOrEmpty (path)) {
				Debug.Log ("XCPlistDocument.createWithPlistPath parameter is unexcept!");
				return null;
			}
			XCPlistDocument plist = new XCPlistDocument ();
			plist.pathForPlist = path;
			try{
				plist.root = (Dictionary<string, object>)PlistCS.Plist.readPlist(path);
			}catch(System.Exception e){
				Debug.LogWarning (e.ToString());
			}
			return plist;
		}
		//对外接口函数
		public void setBool (string key,bool value){
			
			if (string.IsNullOrEmpty (key)) {
				Debug.Log ("XCPlistDocument.setBool parameter is unexcept!");;
				return;
			}
			if(root == null){
				Debug.Log ("XCPlistDocument.root is null");
				return;
			}
			add (key,value);
		}
		public void setString (string key,string value){
			if(string.IsNullOrEmpty(key)){
				Debug.Log ("XCPlistDocument.setString parameter is unexcept!");
			}
			if(root == null){
				Debug.Log ("XCPlistDocument.root is null");
				return;
			}
			add (key,value);
		}
		public void setObject (string key, object value){
			if (string.IsNullOrEmpty (key)) {
				Debug.Log ("XCPlistDocument.setObject parameter is unexcept");
			}
			if(root == null){
				Debug.Log ("XCPlistDocument.root is null");
				return;
			}
			add (key, value);
		}
		public void setDictionary (string key,Dictionary<string,object> value){
			if(string.IsNullOrEmpty(key)){
				Debug.Log ("XCPlitDocument.setDictionary is unpty");
			}
			if(root == null){
				Debug.Log ("XCPlistDocument.root is null");
				return;
			}
			add (key,value);
		}
		//将更改写入磁盘
		public void synchronize(){
			if(root == null){
				Debug.Log ("XCPlistDocument.root is null");
				return;
			}
			if(string.IsNullOrEmpty(pathForPlist)){
				Debug.Log ("XCPlistDocument.pathForPlist is null");
			}
			try{
				PlistCS.Plist.writeXml(root, pathForPlist);
			}
			catch(System.Exception e){
				Debug.Log (e.ToString());
			}
		}
		//utility
		//fix key exist throw exception
		private void add(string key,object value){
			if (string.IsNullOrEmpty (key)) {
				Debug.Log ("XCPlistDocument.add key parameter is unexcept");
				return;
			}
			if(value == null){
				Debug.Log ("XCPlistDocument.add value parameter is unexcept");
				return;
			}
			Debug.Log ("XCPlistDocument.key:"+key);
			Debug.Log ("XCPlistDocument.value:"+value.ToString());
			if (root.ContainsKey (key)) {
				root [key] = value;
			} else {
				root.Add (key,value);
			}
		}
	}
}
 
