using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.EventSystems;

[AddComponentMenu("Camera-Control/Mouse Orbit")]
public class Movecamera : MonoBehaviour
{
	public float distance = 20f;
	public float MaxDist = 250f;
	public float MinDist = 100f;
	public float Zoom = 2f;
	public Transform target;
	private float x;
	public float xSpeed = 250f;
	public float ySpeed = 110f;
	public float speed = 0.1f;
	private float y;
	public int yMaxLimit = 35;
	public float yMinLimit = 12;
	private float last_mouse_x = 0f;
	private float last_mouse_y = 0f;

	//used for double touch
	private const float minPinchDistance = 5;
	public float MaxScale = 3f;
	public float MinScale = 0.3f;

	
	private float pinchDistanceDelta;
	private float pinchDistance;
	public static float ClampAngle(float angle, float min, float max)
	{
		if (angle < -360)
		{
			angle += 360;
		}
		if (angle > 360)
		{
			angle -= 360;
		}
		return Mathf.Clamp(angle, min, max);
	}
	public void OnGUI()
	{
		//var curBtn = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;

		//Debug.Log(curBtn);
		//Debug.Log(EventSystem.current.currentSelectedGameObject.name);
		x = transform.rotation.eulerAngles.y;
		//720°旋转
		//y = transform.rotation.eulerAngles.x;



		//PointerEventData eventData = new PointerEventData(EventSystem.current);
		//eventData.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
		//List<RaycastResult> results = new List<RaycastResult>();
		//EventSystem.current.RaycastAll(eventData, results);

		//for (int i = 0; i < results.Count; i++)
		//{
		//	Debug.Log(results[i].gameObject.name);
		//          if (results[i].gameObject.name == "方向舵")
		//          {

		//          }
		//          else
		//          {

		//          }
		//}

		if (Event.current.type == EventType.MouseDown)
		{
			last_mouse_y = Event.current.mousePosition.y;
			last_mouse_x = Event.current.mousePosition.x;
		}
		if (Event.current.type == EventType.MouseDrag)
		{
			x += speed * (Event.current.mousePosition.x - last_mouse_x);
			y += speed * (Event.current.mousePosition.y - last_mouse_y);
			y = ClampAngle(y, (float)yMinLimit, (float)yMaxLimit);
			Quaternion quaternion = Quaternion.Euler(y, x, (float)0);
			Vector3 vector = ((Vector3)(quaternion * new Vector3((float)0, (float)0, -distance))) + target.position;
			transform.rotation = quaternion;
			transform.position = vector;

			last_mouse_y = Event.current.mousePosition.y;
			last_mouse_x = Event.current.mousePosition.x;
		}
	}
	public void LateUpdate()
	{




		if (Input.touchCount == 2)
		{


			Touch touch1 = Input.touches[0];
			Touch touch2 = Input.touches[1];

			if (touch1.phase == TouchPhase.Moved && touch2.phase == TouchPhase.Moved)
			{
				// ... check the delta distance between them ...
				pinchDistance = Vector2.Distance(touch1.position, touch2.position);
				float prevDistance = Vector2.Distance(touch1.position - touch1.deltaPosition,
					touch2.position - touch2.deltaPosition);
				pinchDistanceDelta = pinchDistance - prevDistance;
				// ... if it's greater than a minimum threshold, it's a pinch!
				if (Mathf.Abs(pinchDistanceDelta) > minPinchDistance)
				{
					zoom(pinchDistanceDelta / Mathf.Abs(pinchDistanceDelta) * -1f);
				}
				else
				{
					pinchDistance = pinchDistanceDelta = 0;
				}
			}
		}
		if (Input.GetAxis("Mouse ScrollWheel") > 0)
		{
			zoom(-Zoom);
		}
		else if (Input.GetAxis("Mouse ScrollWheel") < 0)
		{
			zoom(Zoom);
		}

	}

	public void zoom(float off)
	{
		if (this.enabled)
		{
			if (off > 0 && distance <= MaxDist)
			{
				distance += off;
			}
			else if (off < 0 && distance >= MinDist)
			{
				distance += off;
			}
			if (distance > MaxDist) distance = MaxDist;
			if (distance < MinDist) distance = MinDist;
			/*if(distance - 400 > 0){
				yMinLimit = 12 - (int)(distance - 400)/80;
			} else {
				yMinLimit = 12 + (int)(400 - distance)/20;
			}*/
			x = transform.rotation.eulerAngles.y;
			//y = transform.rotation.eulerAngles.x;
			y = ClampAngle(y, (float)yMinLimit, (float)yMaxLimit);
			Quaternion quaternion = Quaternion.Euler(y, x, (float)0);
			Vector3 vector = ((Vector3)(quaternion * new Vector3((float)0, (float)0, -distance))) + target.position;
			transform.rotation = quaternion;
			transform.position = vector;
		}
	}

	private void zoomto(float dis)
	{
		zoom(dis - distance);
	}

	public void Start()
	{
		Vector3 eulerAngles = transform.eulerAngles;
		x = eulerAngles.y;
		y = eulerAngles.x;

	}

	public void Reset()
	{

	}
}
