using UnityEngine;
using System.Collections;

public class Tester1 : MonoBehaviour {

	WebInfoClient WIC;

	string country="Spain";


	// Use this for initialization
	void Start () {

		WIC = WebInfoClient.instance;
		WIC.GetDataWiki (country);

		WIC.DownloadedInfo+=(string Errors)=> ShowData(Errors);


	}

	private void ShowData(string Errors)
	{
		if (Errors != "") {
			Debug.Log (country + " are " + WIC.Ethnic_groups);
		} else {
			Debug.Log ("Error: "+Errors);
		}
	}

	// Update is called once per frame
	void Update () {
	
	}
}
