using System;
using Log = KSPPP.Logging.ConsoleLogger;


namespace KSPPP { 
	public abstract class ConfigNodeStorage : IPersistenceLoad, IPersistenceSave {

		Func<string> timeNow = () => string.Format("{0:ddMMyyyy-HHmmss}", DateTime.Now);

		public ConfigNodeStorage() { }	// FIXME: should be private?
		public ConfigNodeStorage(string filePath) { FilePath = filePath; } // FIXME: should be private?

		private string _FilePath;
		public string FilePath {
			get { return _FilePath; }
			set { _FilePath = System.IO.Path.Combine(_AssemblyFolder, value).Replace("\\","/"); }
		}
		public string FileName { get { return System.IO.Path.GetFileName(FilePath); } }
		public bool FileExists { get { return System.IO.File.Exists(FilePath); } }

		void IPersistenceLoad.PersistenceLoad() { OnDecodeFromConfigNode(); }
		void IPersistenceSave.PersistenceSave() { OnEncodeToConfigNode(); }

		public virtual void OnDecodeFromConfigNode() { }
		public virtual void OnEncodeToConfigNode() { }

		public bool Load() { return Load(FilePath); }
		public bool Load(string fileFullName) {
			try {
				Log.Debug("Loading ConfigNode");
				if (FileExists) {
					ConfigNode cnToLoad		= ConfigNode.Load(fileFullName);
					ConfigNode cnUnwrapped	= cnToLoad.GetNode(GetType().Name);
					ConfigNode.LoadObjectFromConfig(this, cnUnwrapped);
					return true;
				} else {
					Log.Now("File could not be found to load({0})", fileFullName);
					return false;
				}
			} catch (Exception ex) {
				Log.Now("Failed to Load ConfigNode from file ({0}) - Error:{1}", fileFullName, ex.Message);
				Log.Now("Storing old config - {0}", fileFullName + ".err-" + timeNow);
				System.IO.File.Copy(fileFullName, fileFullName + ".err-" + timeNow,true);
				return false;
			}
		}

		public bool Save() {
			Log.Debug("Saving ConfigNode");
			return Save(FilePath);
		}
		public bool Save(string fileFullName) {
			try {
				ConfigNode cnToSave			= AsConfigNode; 
				ConfigNode cnSaveWrapper	= new ConfigNode(GetType().Name);
				cnSaveWrapper.AddNode(cnToSave); 
				cnSaveWrapper.Save(fileFullName);
				return true;
			} catch (Exception ex) {
				Log.Now("Failed to Save ConfigNode to file({0})-Error:{1}", fileFullName, ex.Message);
				return false;
			}
		}

		public ConfigNode AsConfigNode {
			get {
				try {
					ConfigNode cnTemp = new ConfigNode(GetType().Name);
					cnTemp = ConfigNode.CreateConfigFromObject(this, cnTemp);
					return cnTemp;
				} catch (Exception ex) {
					Log.Now("Failed to generate ConfigNode-Error;{0}", ex.Message);
					return new ConfigNode(GetType ().Name);
				}
			}
		}

		internal static string _AssemblyName		{ get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Name; } }
		internal static string _AssemblyLocation	{ get { return System.Reflection.Assembly.GetExecutingAssembly().Location; } }
		internal static string _AssemblyFolder		{ get { return System.IO.Path.GetDirectoryName(_AssemblyLocation); } }
	}
}
