﻿using System.Collections;
using UnityEditor;

namespace FoundationEditor.Utils.Editor
{
	public class EditorCoroutine
	{
		public static EditorCoroutine start(IEnumerator _routine)
		{
			EditorCoroutine coroutine = new EditorCoroutine(_routine);
			coroutine.start();
			return coroutine;
		}

		readonly IEnumerator routine;

		EditorCoroutine(IEnumerator _routine)
		{
			routine = _routine;
		}

		void start()
		{
			EditorApplication.update += update;
		}
		public void stop()
		{
			EditorApplication.update -= update;
		}

		void update()
		{
			//NOTE: no need to try/catch MoveNext,
			// if an IEnumerator throws its next iteration returns false.
			if (!routine.MoveNext())
			{
				stop();
			}
		}
	}
}
