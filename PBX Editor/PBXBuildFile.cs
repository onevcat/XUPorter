using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityEditor.XCodeEditor
{
	public class PBXBuildFile : PBXObject
	{
		private const string FILE_REF_KEY = "fileRef";
		private const string SETTINGS_KEY = "settings";
		private const string ATTRIBUTES_KEY = "ATTRIBUTES";
		private const string WEAK_VALUE = "Weak";
		private const string COMPILER_FLAGS_KEY = "COMPILER_FLAGS";
		
		public PBXBuildFile( PBXFileReference fileRef, bool weak = false, string[] compilerFlags = null ) : base()
		{
			this.Add( FILE_REF_KEY, fileRef.guid );
			SetWeakLink( weak );
			AddCompilerFlags(compilerFlags);
		}
		
		public PBXBuildFile( string guid, PBXDictionary dictionary ) : base ( guid, dictionary )
		{
		}

		public string fileRef
		{
			get {
				return (string)_data[ FILE_REF_KEY ];
			}
		}
		
		public bool SetWeakLink( bool weak = false )
		{
			PBXDictionary settings = null;
			PBXList attributes = null;
			
			if( !_data.ContainsKey( SETTINGS_KEY ) ) {
				if( weak ) {
					attributes = new PBXList();
					attributes.Add( WEAK_VALUE );
					
					settings = new PBXDictionary();
					settings.Add( ATTRIBUTES_KEY, attributes );

					_data.Add( SETTINGS_KEY, settings );
				}
				return true;
			}
			
			settings = _data[ SETTINGS_KEY ] as PBXDictionary;
			if( !settings.ContainsKey( ATTRIBUTES_KEY ) ) {
				if( weak ) {
					attributes = new PBXList();
					attributes.Add( WEAK_VALUE );
					settings.Add( ATTRIBUTES_KEY, attributes );
					return true;
				}
				else {
					return false;
				}
			}
			else {
				attributes = settings[ ATTRIBUTES_KEY ] as PBXList;
			}

			attributes.Remove( WEAK_VALUE );
			if( weak ) {
				attributes.Add( WEAK_VALUE );
			}
			
			return true;
		}

		//CodeSignOnCopy
		public bool AddCodeSignOnCopy()
		{
			if( !_data.ContainsKey( SETTINGS_KEY ) )
				_data[ SETTINGS_KEY ] = new PBXDictionary();

			var settings = _data[ SETTINGS_KEY ] as PBXDictionary;
			if( !settings.ContainsKey( ATTRIBUTES_KEY ) ) {
				var attributes = new PBXList();
				attributes.Add( "CodeSignOnCopy" );
				attributes.Add( "RemoveHeadersOnCopy" );
				settings.Add( ATTRIBUTES_KEY, attributes );
			}
			else {
				var attributes = settings[ ATTRIBUTES_KEY ] as PBXList;
				attributes.Add( "CodeSignOnCopy" );
				attributes.Add( "RemoveHeadersOnCopy" );
			}
			return true;		
		}

		
		public bool AddCompilerFlags( params string[] flags )
		{
			if( flags == null || flags.Length == 0)
				return false;

            object settingsObject;
			PBXDictionary settings;

            if( !_data.TryGetValue( SETTINGS_KEY, out settingsObject ) ) {
				settings = new PBXDictionary();
				_data[ SETTINGS_KEY ] = settings;
			} else
                settings = settingsObject as PBXDictionary;
			
			List<string> currentFlags = null;
			if( settings.ContainsKey( COMPILER_FLAGS_KEY ) ) {
                // merge specified with existing
				currentFlags = new List<string>(((string)settings[ COMPILER_FLAGS_KEY ]).Split( ' ' ));

				foreach( string flag in flags ) {
					if( !currentFlags.Contains(flag) )
                        currentFlags.Add(flag);
				}
			} else {
				// no current flags so just use ones specified
				currentFlags = new List<string>(flags);
			}

			settings[ COMPILER_FLAGS_KEY ] = string.Join( " ", currentFlags.ToArray() );
			return true;
		}
		
	}
}
