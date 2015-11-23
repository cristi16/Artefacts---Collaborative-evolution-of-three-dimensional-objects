/**
 *	\brief Hax!  DLLs cannot interpret preprocessor directives, so this class acts as a "bridge"
 */
using System;
using UnityEngine;
using System.Collections;

namespace DigitalOpus.MB.Core{

	public interface MBVersionInterface{
		string version();
		int GetMajorVersion();
		int GetMinorVersion();
		bool GetActive(GameObject go);
		void SetActive(GameObject go, bool isActive);
		void SetActiveRecursively(GameObject go, bool isActive);
		UnityEngine.Object[] FindSceneObjectsOfType(Type t);
		bool IsRunningAndMeshNotReadWriteable(Mesh m);
		Vector2[] GetMeshUV1s(Mesh m, MB2_LogLevel LOG_LEVEL);
		void MeshClear(Mesh m, bool t);
		void MeshAssignUV1(Mesh m, Vector2[] uv1s);
		Vector4 GetLightmapTilingOffset(Renderer r);
		Transform[] GetBones(Renderer r);
	}

	public class MBVersion
	{
		private static MBVersionInterface _MBVersion;

		private static MBVersionInterface _CreateMBVersionConcrete(){
			Type vit = null;
#if EVAL_VERSION
			vit = Type.GetType("DigitalOpus.MB.Core.MBVersionConcrete,Assembly-CSharp");
#else
			vit = typeof(MBVersionConcrete);
#endif
			return (MBVersionInterface) Activator.CreateInstance(vit);
		}

		public static string version(){
			if (_MBVersion == null) _MBVersion = _CreateMBVersionConcrete();
			return _MBVersion.version();
		}
		
		public static int GetMajorVersion(){
			if (_MBVersion == null) _MBVersion = _CreateMBVersionConcrete();
			return _MBVersion.GetMajorVersion();	
		}

		public static int GetMinorVersion(){
			if (_MBVersion == null) _MBVersion = _CreateMBVersionConcrete();
			return _MBVersion.GetMinorVersion();
		}

		public static bool GetActive(GameObject go){
			if (_MBVersion == null) _MBVersion = _CreateMBVersionConcrete();
			return _MBVersion.GetActive(go);
		}
	
		public static void SetActive(GameObject go, bool isActive){
			if (_MBVersion == null) _MBVersion = _CreateMBVersionConcrete();
			_MBVersion.SetActive(go,isActive);
		}
		
		public static void SetActiveRecursively(GameObject go, bool isActive){
			if (_MBVersion == null) _MBVersion = _CreateMBVersionConcrete();
			_MBVersion.SetActiveRecursively(go,isActive);
		}

		public static UnityEngine.Object[] FindSceneObjectsOfType(Type t){
			if (_MBVersion == null) _MBVersion = _CreateMBVersionConcrete();
			return _MBVersion.FindSceneObjectsOfType(t);				
		}

		public static bool IsRunningAndMeshNotReadWriteable(Mesh m){
			if (_MBVersion == null) _MBVersion = _CreateMBVersionConcrete();
			return _MBVersion.IsRunningAndMeshNotReadWriteable(m);
		}

		public static Vector2[] GetMeshUV1s(Mesh m, MB2_LogLevel LOG_LEVEL){
			if (_MBVersion == null) _MBVersion = _CreateMBVersionConcrete();
			return _MBVersion.GetMeshUV1s(m,LOG_LEVEL);
		}

		public static void MeshClear(Mesh m, bool t){
			if (_MBVersion == null) _MBVersion = _CreateMBVersionConcrete();
			_MBVersion.MeshClear(m,t);
		}

		public static void MeshAssignUV1(Mesh m, Vector2[] uv1s){
			if (_MBVersion == null) _MBVersion = _CreateMBVersionConcrete();
			_MBVersion.MeshAssignUV1(m,uv1s);
		}

		public static Vector4 GetLightmapTilingOffset(Renderer r){
			if (_MBVersion == null) _MBVersion = _CreateMBVersionConcrete();
			return _MBVersion.GetLightmapTilingOffset(r);
		}

		public static Transform[] GetBones(Renderer r){
			if (_MBVersion == null) _MBVersion = _CreateMBVersionConcrete();
			return _MBVersion.GetBones(r);
		}
	}
}