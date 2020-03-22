using UnityEngine;

namespace Touches
{
	/// <summary>
	/// GodTouchを参考にした マウスクリックをスマホにも対応可能にするためのクラス。
	/// </summary>
	public static class TouchInput
	{
		/// <summary>
		/// Androidフラグ
		/// </summary>
		static readonly bool IsAndroid = Application.platform == RuntimePlatform.Android;
		/// <summary>
		/// iOSフラグ
		/// </summary>
		static readonly bool IsIOS = Application.platform == RuntimePlatform.IPhonePlayer;
		/// <summary>
		/// PCフラグ
		/// </summary> 
		static readonly bool IsPC = !IsAndroid && !IsIOS;

		/// <summary>
		/// デルタポジション判定用・前回のポジション
		/// </summary>
		static Vector3 previousPosition;

		/// <summary>
		/// タッチ情報を取得(エディタとスマホを考慮)
		/// </summary>
		/// <returns>タッチ情報</returns>
		public static TouchInfo GetTouchPhase()
		{
			if (IsPC)
			{
				if (Input.GetMouseButtonDown(0))
				{
					previousPosition = Input.mousePosition;
					return TouchInfo.Down;
				}
				else if (Input.GetMouseButton(0))
				{
					return TouchInfo.Moved;
				}
				else if (Input.GetMouseButtonUp(0))
				{
					return TouchInfo.Up;
				}
			}
			else
			{
				if (Input.touchCount > 0) return (TouchInfo)((int)Input.GetTouch(0).phase);
			}
			return TouchInfo.None;
		}

		/// <summary>
		/// タッチポジションを取得(エディタとスマホを考慮)
		/// </summary>
		/// <returns>タッチポジション。タッチされていない場合は (0, 0, 0)</returns>
		public static Vector3 GetPosition()
		{
			if (IsPC)
			{
				if (GetTouchPhase() != TouchInfo.None) return Input.mousePosition;
			}
			else
			{
				if (Input.touchCount > 0) return Input.GetTouch(0).position;
			}
			return Vector3.zero;
		}

		/// <summary>
		/// タッチデルタポジションを取得(エディタとスマホを考慮)
		/// </summary>
		/// <returns>タッチポジション。タッチされていない場合は (0, 0, 0)</returns>
		public static Vector3 GetDeltaPosition()
		{
			if (IsPC)
			{
				var phase = GetTouchPhase();
				if (phase != TouchInfo.None)
				{
					var now = Input.mousePosition;
					var delta = now - previousPosition;
					previousPosition = now;
					return delta;
				}
			}
			else
			{
				if (Input.touchCount > 0) return Input.GetTouch(0).deltaPosition;
			}
			return Vector3.zero;
		}
	}

	/// <summary>
	/// タッチ情報。UnityEngine.TouchPhase に None の情報を追加拡張。
	/// </summary>
	public enum TouchInfo
	{
		None = -1,
		Down = 0,
		Moved = 1,
		Stationary = 2,
		Up = 3,
		Canceled = 4
	}
}