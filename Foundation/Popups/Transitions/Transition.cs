using System;
using UnityEngine;

namespace Foundation.Popups.Transitions
{
	public abstract class Transition : MonoBehaviour
	{
		public abstract void In(Action onCompleated);
		public abstract void Out(Action onCompleated);

	}
}
