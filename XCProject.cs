using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace UnityEditor.XCodeEditor
{
	public partial class XCProject : System.IDisposable
	{
		private PBXDictionary _datastore;
		public PBXDictionary _objects;
		private PBXDictionary _configurations;
		
		private PBXGroup _rootGroup;
		private string _defaultConfigurationName;
		private string _rootObjectKey;
	
		public string projectRootPath { get; private set; }
		private FileInfo projectFileInfo;
		
		public string filePath { get; private set; }
		private string sourcePathRoot;
		private bool modified = false;
		
		#region Data
		
		// Objects
		private PBXSortedDictionary<PBXBuildFile> _buildFiles;
		private PBXSortedDictionary<PBXGroup> _groups;
		private PBXSortedDictionary<PBXFileReference> _fileReferences;
		private PBXDictionary<PBXNativeTarget> _nativeTargets;
		
		private PBXDictionary<PBXFrameworksBuildPhase> _frameworkBuildPhases;
		private PBXDictionary<PBXResourcesBuildPhase> _resourcesBuildPhases;
		private PBXDictionary<PBXShellScriptBuildPhase> _shellScriptBuildPhases;
		private PBXDictionary<PBXSourcesBuildPhase> _sourcesBuildPhases;
		private PBXDictionary<PBXCopyFilesBuildPhase> _copyBuildPhases;
				
		private PBXDictionary<XCBuildConfiguration> _buildConfigurations;
		private PBXSortedDictionary<XCConfigurationList> _configurationLists;
		
		private PBXProject _project;
		
		#endregion


		#region Constructor
		
		public XCProject()
		{
			
		}
		
		public XCProject( string filePath ) : this()
		{
			if( !System.IO.Directory.Exists( filePath ) ) {
				Debug.LogWarning( "Path does not exists." );
				return;
			}
			
			if( filePath.EndsWith( ".xcodeproj" ) ) {
				Debug.Log( "Opening project " + filePath );
				this.projectRootPath = Path.GetDirectoryName( filePath );
				this.filePath = filePath;
			} else {
				Debug.Log( "Looking for xcodeproj files in " + filePath );
				string[] projects = System.IO.Directory.GetDirectories( filePath, "*.xcodeproj" );
				if( projects.Length == 0 ) {
					Debug.LogWarning( "Error: missing xcodeproj file" );
					return;
				}
				
				this.projectRootPath = filePath;
				this.filePath = projects[ 0 ];
			}
			
			projectFileInfo = new FileInfo( Path.Combine( this.filePath, "project.pbxproj" ) );
			string contents = projectFileInfo.OpenText().ReadToEnd();
			
			PBXParser parser = new PBXParser();
			_datastore = parser.Decode( contents );
			if( _datastore == null ) {
				throw new System.Exception( "Project file not found at file path " + filePath );
			}

			if( !_datastore.ContainsKey( "objects" ) ) {
				Debug.Log( "Errore " + _datastore.Count );
				return;
			}
			
			_objects = (PBXDictionary)_datastore["objects"];
			modified = false;
			
			_rootObjectKey = (string)_datastore["rootObject"];
			if( !string.IsNullOrEmpty( _rootObjectKey ) ) {
				_project = new PBXProject( _rootObjectKey, (PBXDictionary)_objects[ _rootObjectKey ] );
				_rootGroup = new PBXGroup( _rootObjectKey, (PBXDictionary)_objects[ _project.mainGroupID ] );
			}
			else {
				Debug.LogWarning( "error: project has no root object" );
				_project = null;
				_rootGroup = null;
			}

		}
		
		#endregion


		#region Properties
		
		public PBXProject project {
			get {
				return _project;
			}
		}
		
		public PBXGroup rootGroup {
			get {
				return _rootGroup;
			}
		}
		
		public PBXSortedDictionary<PBXBuildFile> buildFiles {
			get {
				if( _buildFiles == null ) {
					_buildFiles = new PBXSortedDictionary<PBXBuildFile>( _objects );
				}
				return _buildFiles;
			}
		}
		
		public PBXSortedDictionary<PBXGroup> groups {
			get {
				if( _groups == null ) {
					_groups = new PBXSortedDictionary<PBXGroup>( _objects );
				}
				return _groups;
			}
		}
		
		public PBXSortedDictionary<PBXFileReference> fileReferences {
			get {
				if( _fileReferences == null ) {
					_fileReferences = new PBXSortedDictionary<PBXFileReference>( _objects );
				}
				return _fileReferences;
			}
		}
		
		public PBXDictionary<PBXNativeTarget> nativeTargets {
			get {
				if( _nativeTargets == null ) {
					_nativeTargets = new PBXDictionary<PBXNativeTarget>( _objects );
				}
				return _nativeTargets;
			}
		}
		
		public PBXDictionary<XCBuildConfiguration> buildConfigurations {
			get {
				if( _buildConfigurations == null ) {
					_buildConfigurations = new PBXDictionary<XCBuildConfiguration>( _objects );
				}
				return _buildConfigurations;
			}
		}
		
		public PBXSortedDictionary<XCConfigurationList> configurationLists {
			get {
				if( _configurationLists == null ) {
					_configurationLists = new PBXSortedDictionary<XCConfigurationList>( _objects );
				}
				return _configurationLists;
			}
		}
		
		public PBXDictionary<PBXFrameworksBuildPhase> frameworkBuildPhases {
			get {
				if( _frameworkBuildPhases == null ) {
					_frameworkBuildPhases = new PBXDictionary<PBXFrameworksBuildPhase>( _objects );
				}
				return _frameworkBuildPhases;
			}
		}
	
		public PBXDictionary<PBXResourcesBuildPhase> resourcesBuildPhases {
			get {
				if( _resourcesBuildPhases == null ) {
					_resourcesBuildPhases = new PBXDictionary<PBXResourcesBuildPhase>( _objects );
				}
				return _resourcesBuildPhases;
			}
		}
	
		public PBXDictionary<PBXShellScriptBuildPhase> shellScriptBuildPhases {
			get {
				if( _shellScriptBuildPhases == null ) {
					_shellScriptBuildPhases = new PBXDictionary<PBXShellScriptBuildPhase>( _objects );
				}
				return _shellScriptBuildPhases;
			}
		}
	
		public PBXDictionary<PBXSourcesBuildPhase> sourcesBuildPhases {
			get {
				if( _sourcesBuildPhases == null ) {
					_sourcesBuildPhases = new PBXDictionary<PBXSourcesBuildPhase>( _objects );
				}
				return _sourcesBuildPhases;
			}
		}
	
		public PBXDictionary<PBXCopyFilesBuildPhase> copyBuildPhases {
			get {
				if( _copyBuildPhases == null ) {
					_copyBuildPhases = new PBXDictionary<PBXCopyFilesBuildPhase>( _objects );
				}
				return _copyBuildPhases;
			}
		}

		#endregion


		#region PBXMOD
		
		public bool AddOtherCFlags( string flag )
		{
			return AddOtherCFlags( new PBXList( flag ) ); 
		}
		
		public bool AddOtherCFlags( PBXList flags )
		{
			foreach( KeyValuePair<string, XCBuildConfiguration> buildConfig in buildConfigurations ) {
				buildConfig.Value.AddOtherCFlags( flags );
			}
			modified = true;
			return modified;	
		}

		public bool AddOtherLinkerFlags( string flag )
		{
			return AddOtherLinkerFlags( new PBXList( flag ) ); 
		}
		
		public bool AddOtherLinkerFlags( PBXList flags )
		{
			foreach( KeyValuePair<string, XCBuildConfiguration> buildConfig in buildConfigurations ) {
				buildConfig.Value.AddOtherLinkerFlags( flags );
			}
			modified = true;
			return modified;	
		}
		
		public bool AddHeaderSearchPaths( string path )
		{
			return AddHeaderSearchPaths( new PBXList( path ) );
		}
		
		public bool AddHeaderSearchPaths( PBXList paths )
		{
			foreach( KeyValuePair<string, XCBuildConfiguration> buildConfig in buildConfigurations ) {
				buildConfig.Value.AddHeaderSearchPaths( paths );
			}
			modified = true;
			return modified;
		}
		
		public bool AddLibrarySearchPaths( string path )
		{
			return AddLibrarySearchPaths( new PBXList( path ) );
		}
		
		public bool AddLibrarySearchPaths( PBXList paths )
		{
			foreach( KeyValuePair<string, XCBuildConfiguration> buildConfig in buildConfigurations ) {
				buildConfig.Value.AddLibrarySearchPaths( paths );
			}
			modified = true;
			return modified;
		}
		
		public bool AddFrameworkSearchPaths( string path )
		{
			return AddFrameworkSearchPaths( new PBXList( path ) );
		}

		public bool AddFrameworkSearchPaths( PBXList paths )
		{
			foreach( KeyValuePair<string, XCBuildConfiguration> buildConfig in buildConfigurations ) {
				buildConfig.Value.AddFrameworkSearchPaths( paths );
			}
			modified = true;
			return modified;
		}
		
		public object GetObject( string guid )
		{
			return _objects[guid];
		}
		
		public PBXDictionary AddFile( string filePath, PBXGroup parent = null, string tree = "SOURCE_ROOT", bool createBuildFiles = true, bool weak = false )
		{
			PBXDictionary results = new PBXDictionary();
			string absPath = string.Empty;
			
			if( Path.IsPathRooted( filePath ) ) {
				absPath = filePath;
			}
			else if( tree.CompareTo( "SDKROOT" ) != 0) {
				absPath = Path.Combine( Application.dataPath, filePath );
			}
			
			if( !( File.Exists( absPath ) || Directory.Exists( absPath ) ) && tree.CompareTo( "SDKROOT" ) != 0 ) {
				Debug.Log( "Missing file: " + filePath );
				return results;
			}
			else if( tree.CompareTo( "SOURCE_ROOT" ) == 0 ) {
				System.Uri fileURI = new System.Uri( absPath );
				System.Uri rootURI = new System.Uri( ( projectRootPath + "/." ) );
				filePath = rootURI.MakeRelativeUri( fileURI ).ToString();
			}
			
			if( parent == null ) {
				parent = _rootGroup;
			}
			
			//Check if there is already a file
			PBXFileReference fileReference = GetFile( System.IO.Path.GetFileName( filePath ) );	
			if( fileReference != null ) {
				Debug.LogWarning("File is already exists: " + filePath);
				return null;
			}
			
			fileReference = new PBXFileReference( filePath, (TreeEnum)System.Enum.Parse( typeof(TreeEnum), tree ) );
			parent.AddChild( fileReference );
			fileReferences.Add( fileReference );
			results.Add( fileReference.guid, fileReference );
			
			//Create a build file for reference
			if( !string.IsNullOrEmpty( fileReference.buildPhase ) && createBuildFiles ) {
				
				switch( fileReference.buildPhase ) {
					case "PBXFrameworksBuildPhase":
						foreach( KeyValuePair<string, PBXFrameworksBuildPhase> currentObject in frameworkBuildPhases ) {
							BuildAddFile(fileReference,currentObject,weak);
						}
						if ( !string.IsNullOrEmpty( absPath ) && ( tree.CompareTo( "SOURCE_ROOT" ) == 0 )) {
							string libraryPath = Path.Combine( "$(SRCROOT)", Path.GetDirectoryName( filePath ) );
							if (File.Exists(absPath)) {
								this.AddLibrarySearchPaths( new PBXList( libraryPath ) ); 
							} else {
								this.AddFrameworkSearchPaths( new PBXList( libraryPath ) );  
							}
							
						}
						break;
					case "PBXResourcesBuildPhase":
						foreach( KeyValuePair<string, PBXResourcesBuildPhase> currentObject in resourcesBuildPhases ) {
							BuildAddFile(fileReference,currentObject,weak);
						}
						break;
					case "PBXShellScriptBuildPhase":
						foreach( KeyValuePair<string, PBXShellScriptBuildPhase> currentObject in shellScriptBuildPhases ) {
							BuildAddFile(fileReference,currentObject,weak);
						}
						break;
					case "PBXSourcesBuildPhase":
						foreach( KeyValuePair<string, PBXSourcesBuildPhase> currentObject in sourcesBuildPhases ) {
							BuildAddFile(fileReference,currentObject,weak);
						}
						break;
					case "PBXCopyFilesBuildPhase":
						foreach( KeyValuePair<string, PBXCopyFilesBuildPhase> currentObject in copyBuildPhases ) {
							BuildAddFile(fileReference,currentObject,weak);
						}
						break;
					case null:
						Debug.LogWarning( "File Not Support: " + filePath );
						break;
					default:
						Debug.LogWarning( "File Not Support." );
						return null;
				}
			}
			return results;
		}

		private void BuildAddFile (PBXFileReference fileReference, KeyValuePair<string, PBXFrameworksBuildPhase> currentObject,bool weak)
		{
			PBXBuildFile buildFile = new PBXBuildFile( fileReference, weak );
			buildFiles.Add( buildFile );
			currentObject.Value.AddBuildFile( buildFile );
		}
		private void BuildAddFile (PBXFileReference fileReference, KeyValuePair<string, PBXResourcesBuildPhase> currentObject,bool weak)
		{
			PBXBuildFile buildFile = new PBXBuildFile( fileReference, weak );
			buildFiles.Add( buildFile );
			currentObject.Value.AddBuildFile( buildFile );
		}
		private void BuildAddFile (PBXFileReference fileReference, KeyValuePair<string, PBXShellScriptBuildPhase> currentObject,bool weak)
		{
			PBXBuildFile buildFile = new PBXBuildFile( fileReference, weak );
			buildFiles.Add( buildFile );
			currentObject.Value.AddBuildFile( buildFile );
		}
		private void BuildAddFile (PBXFileReference fileReference, KeyValuePair<string, PBXSourcesBuildPhase> currentObject,bool weak)
		{
			PBXBuildFile buildFile = new PBXBuildFile( fileReference, weak );
			buildFiles.Add( buildFile );
			currentObject.Value.AddBuildFile( buildFile );
		}
		private void BuildAddFile (PBXFileReference fileReference, KeyValuePair<string, PBXCopyFilesBuildPhase> currentObject,bool weak)
		{
			PBXBuildFile buildFile = new PBXBuildFile( fileReference, weak );
			buildFiles.Add( buildFile );
			currentObject.Value.AddBuildFile( buildFile );
		}
		
		public bool AddFolder( string folderPath, PBXGroup parent = null, string[] exclude = null, bool recursive = true, bool createBuildFile = true )
		{
			if( !Directory.Exists( folderPath ) )
				return false;
			DirectoryInfo sourceDirectoryInfo = new DirectoryInfo( folderPath );
			
			if( exclude == null )
				exclude = new string[] {};
			
			if( parent == null )
				parent = rootGroup;
			
			// Create group
			PBXGroup newGroup = GetGroup( sourceDirectoryInfo.Name, null /*relative path*/, parent );
			
			foreach( string directory in Directory.GetDirectories( folderPath ) ) {
				Debug.Log( "DIR: " + directory );
				if( directory.EndsWith( ".bundle" ) ) {
					// Treath it like a file and copy even if not recursive
					// TODO also for .xcdatamodeld?
					Debug.LogWarning( "This is a special folder: " + directory );
					AddFile( directory, newGroup, "SOURCE_ROOT", createBuildFile );
					continue;
				}
				
				if( recursive ) {
					Debug.Log( "recursive" );
					AddFolder( directory, newGroup, exclude, recursive, createBuildFile );
				}
			}
			
			// Adding files.
			string regexExclude = string.Format( @"{0}", string.Join( "|", exclude ) );
			foreach( string file in Directory.GetFiles( folderPath ) ) {
				if( Regex.IsMatch( file, regexExclude ) ) {
					continue;
				}
				AddFile( file, newGroup, "SOURCE_ROOT", createBuildFile );
			}
			
			modified = true;
			return modified;
		}
		#endregion

		#region Getters
		public PBXFileReference GetFile( string name )
		{
			if( string.IsNullOrEmpty( name ) ) {
				return null;
			}
			
			foreach( KeyValuePair<string, PBXFileReference> current in fileReferences ) {
				if( !string.IsNullOrEmpty( current.Value.name ) && current.Value.name.CompareTo( name ) == 0 ) {
					return current.Value;
				}
			}
			
			return null;
		}
		
		public PBXGroup GetGroup( string name, string path = null, PBXGroup parent = null )
		{
			if( string.IsNullOrEmpty( name ) )
				return null;
			
			if( parent == null ) parent = rootGroup;
			
			foreach( KeyValuePair<string, PBXGroup> current in groups ) {
				if( string.IsNullOrEmpty( current.Value.name ) ) { 
					if( current.Value.path.CompareTo( name ) == 0 && parent.HasChild( current.Key ) ) {
						return current.Value;
					}
				} else if( current.Value.name.CompareTo( name ) == 0 && parent.HasChild( current.Key ) ) {
					return current.Value;
				}
			}

			PBXGroup result = new PBXGroup( name, path );
			groups.Add( result );
			parent.AddChild( result );
			
			modified = true;
			return result;
		}
			
		#endregion

		#region Mods		
		public void ApplyMod( string pbxmod )
		{
			XCMod mod = new XCMod( pbxmod );
			ApplyMod( mod );
		}
		
		public void ApplyMod( XCMod mod )
		{	
			PBXGroup modGroup = this.GetGroup( mod.group );
			
			Debug.Log( "Adding libraries..." );
			
			foreach( XCModFile libRef in mod.libs ) {
				string completeLibPath = System.IO.Path.Combine( "usr/lib", libRef.filePath );
				this.AddFile( completeLibPath, modGroup, "SDKROOT", true, libRef.isWeak );
			}
			
			Debug.Log( "Adding frameworks..." );
			PBXGroup frameworkGroup = this.GetGroup( "Frameworks" );
			foreach( string framework in mod.frameworks ) {
				string[] filename = framework.Split( ':' );
				bool isWeak = ( filename.Length > 1 ) ? true : false;
				string completePath = System.IO.Path.Combine( "System/Library/Frameworks", filename[0] );
				this.AddFile( completePath, frameworkGroup, "SDKROOT", true, isWeak );
			}
			
			Debug.Log( "Adding files..." );
			foreach( string filePath in mod.files ) {
				string absoluteFilePath = System.IO.Path.Combine( mod.path, filePath );
				this.AddFile( absoluteFilePath, modGroup );
			}
			
			Debug.Log( "Adding folders..." );
			foreach( string folderPath in mod.folders ) {
				string absoluteFolderPath = System.IO.Path.Combine( mod.path, folderPath );
				this.AddFolder( absoluteFolderPath, modGroup, (string[])mod.excludes.ToArray( typeof(string) ) );
			}
			
			Debug.Log( "Adding headerpaths..." );
			foreach( string headerpath in mod.headerpaths ) {
				string absoluteHeaderPath = System.IO.Path.Combine( mod.path, headerpath );
				this.AddHeaderSearchPaths( absoluteHeaderPath );
			}

			Debug.Log( "Adding linker flags..." );
			foreach( string flag in mod.linker_flags ) {
				this.AddOtherLinkerFlags( flag );
			}
			
			this.Consolidate();
		}
		
		#endregion

		#region Savings	
		public void Consolidate()
		{
			PBXDictionary consolidated = new PBXDictionary();
			consolidated.Append<PBXBuildFile>( this.buildFiles );//sort!
			consolidated.Append<PBXCopyFilesBuildPhase>( this.copyBuildPhases );
			consolidated.Append<PBXFileReference>( this.fileReferences );//sort!
			consolidated.Append<PBXFrameworksBuildPhase>( this.frameworkBuildPhases );
			consolidated.Append<PBXGroup>( this.groups );//sort!
			consolidated.Append<PBXNativeTarget>( this.nativeTargets );
			consolidated.Add( project.guid, project.data );//TODO this should be named PBXProject?
			consolidated.Append<PBXResourcesBuildPhase>( this.resourcesBuildPhases );
			consolidated.Append<PBXShellScriptBuildPhase>( this.shellScriptBuildPhases );
			consolidated.Append<PBXSourcesBuildPhase>( this.sourcesBuildPhases );
			consolidated.Append<XCBuildConfiguration>( this.buildConfigurations );
			consolidated.Append<XCConfigurationList>( this.configurationLists );
			_objects = consolidated;
			consolidated = null;
		}

		public void Backup()
		{
			string backupPath = Path.Combine( this.filePath, "project.backup.pbxproj" );
			
			// Delete previous backup file
			if( File.Exists( backupPath ) )
				File.Delete( backupPath );
			
			// Backup original pbxproj file first
			File.Copy( System.IO.Path.Combine( this.filePath, "project.pbxproj" ), backupPath );
		}

		private void DeleteExisting(string path)
		{
            // Delete old project file
            if( File.Exists( path ))
                File.Delete( path );
		}

		private void CreateNewProject(PBXDictionary result, string path) 
		{
			PBXParser parser = new PBXParser();
			StreamWriter saveFile = File.CreateText( path );
			saveFile.Write( parser.Encode( result, true ) );
			saveFile.Close();
		}
		
		/// <summary>
		/// Saves a project after editing.
		/// </summary>
		public void Save()
		{
			PBXDictionary result = new PBXDictionary();
			result.Add( "archiveVersion", 1 );
			result.Add( "classes", new PBXDictionary() );
			result.Add( "objectVersion", 46 );
			
			Consolidate();
			result.Add( "objects", _objects );
			
			result.Add( "rootObject", _rootObjectKey );
			
			string projectPath = Path.Combine( this.filePath, "project.pbxproj" );

			// Delete old project file, in case of an IOException 'Sharing violation on path Error'
			DeleteExisting(projectPath);

			// Parse result object directly into file
			CreateNewProject(result,projectPath);
		}
		
		/**
		* Raw project data.
		*/
		public Dictionary<string, object> objects {
			get {
				return null;
			}
		}
		#endregion

		public void Dispose()
   		{
   		
	   	}
	}
}
