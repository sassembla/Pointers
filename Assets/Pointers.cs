using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using UnityEngine;

public class BaseClass {
	public void BaseMethod() {
		Debug.LogError("BaseMethod.");
	}
}

public class NewClass {
	public void NewMethod() {
		Debug.Log("NewMethod!");
	}
}

public class Pointers : MonoBehaviour {

	// Use this for initialization
	void Start () {
		PointerSwap();
		MethodILGenerate();
	}

	void PointerSwap () {
		var baseMethod_MethodInfo = typeof(BaseClass).GetMethod("BaseMethod", BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
		var newMethod_MethodInfo = typeof(NewClass).GetMethod("NewMethod", BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

		RuntimeHelpers.PrepareMethod(baseMethod_MethodInfo.MethodHandle);
		RuntimeHelpers.PrepareMethod(newMethod_MethodInfo.MethodHandle);

		unsafe {
			// ready void* pointers.
			var basePointer = baseMethod_MethodInfo.MethodHandle.Value.ToPointer();
			var newPointer = newMethod_MethodInfo.MethodHandle.Value.ToPointer();
			
			// swap method pointer, *base ---> *new.
			*((Int64*)new IntPtr(basePointer).ToPointer()) = *((Int64*)new IntPtr(newPointer).ToPointer());
		}

		var baseInstance = new BaseClass();
		baseInstance.BaseMethod();// エディタではNewMethodが実行されるが、実機では元のBaseMethodが実行される。。。

		baseMethod_MethodInfo.Invoke(baseInstance, new object[]{});// この呼び方だと実機でもエディタでもNewMethodが実行される。
	}

	void MethodILGenerate () {
		try {
			var dMethod = new DynamicMethod("test", typeof(void), new Type[]{});

			var il = dMethod.GetILGenerator();
			il.EmitWriteLine("hello!!");
			il.Emit(OpCodes.Ret);

			var testDelegate = (Test.TestDelegate)dMethod.CreateDelegate(typeof(Test.TestDelegate));
			testDelegate();
		} catch (Exception e) {
			Debug.LogError("e:" + e);
		}
	}
}

public class Test {
	public delegate void TestDelegate();
}
