using UnityEngine;
using System.Collections;
using System.Net;
using UnityEngine.UI;
using HtmlAgilityPack;
using System.IO;
using System;


public delegate void DownloadedInfo(string Error);
public delegate void DownloadedAnthem(string Error);
public delegate void DownloadedFlag(string Error);



/// <summary>
/// Web info client.
/// </summary>
[Serializable]
public class WebInfoClient : MonoBehaviour {

	static WebInfoClient _instance;

	public event DownloadedInfo DownloadedInfo;
	public event DownloadedAnthem DownloadedAnthem;
	public event DownloadedFlag DownloadedFlag;

	/// <summary>
	/// Instance this instance.
	/// </summary>
	public static WebInfoClient instance
	{
		get{ 
			if (_instance == null) {
				GameObject obj = GameObject.Find ("WikiParser");

				if (obj == null) {
					Debug.LogWarning ("'WikiParser' GameObject could not be found in the scene. Make sure it's created with this name before using any functionality.");
				} else {
					_instance = obj.GetComponent<WebInfoClient> ();
				}
			}
			return _instance;
		}

	}


	#region Components
		public InputField  inputtext; 
		public InputField  memo; 
		public Image flag;
		public AudioSource AudioAnthem;
	#endregion

	/// <summary>
	/// It stores the last error.
	/// </summary>
	private string Error;


	public string 	Country,
					Population,
					Official_language,
					Flag_URL,
					Ethnic_groups,
					Anthem;





	// Use this for initialization
	void Start () {


		CleanData ();

			
	}

	/// <summary>
	/// Cleans the data.
	/// </summary>
	void CleanData ()
	{
		Debug.ClearDeveloperConsole ();
		Official_language = "";
		Flag_URL = "";
		memo.text = "";
		Anthem = "";
		Ethnic_groups = "";
		Population = "";
		Country = "";
	}


	public void PlayAnthem()
	{
//		WWW wwwAnthem = new WWW(Anthem);
//
//		StartCoroutine (WaitForRequestplay (wwwAnthem));
//
//		if (AudioAnthem.clip!=null)
//		if ((!AudioAnthem.isPlaying) &&
//			(AudioAnthem.clip.loadState == AudioDataLoadState.Loaded)) {
//
//			AudioAnthem.Play ();
//
//		}
	}



	/// <summary>
	/// Waits for requestplay.
	/// </summary>
	/// <returns>The for requestplay.</returns>
	/// <param name="www">Www.</param>
	private IEnumerator WaitForRequestplay(WWW www){
		yield return www;

		bool HaveFile = false;

		while ((www.error!="")&&(!HaveFile))
			{
				if (www.audioClip!=null){
					AudioAnthem.clip = www.GetAudioClip(false,false,AudioType.OGGVORBIS);
				}else{
					HaveFile=true;
				}

			}


	}



	/// <summary>
	/// Parse the wikipedia website to obtain the data.
	/// </summary>
    public void GetDataWiki()
    {

		CleanData ();



		if (inputtext.text != "") {
			string url = "https://en.wikipedia.org/wiki/" + inputtext.text;
			
			memo.text += "url: " + url +"\r\n";
			Debug.Log (url);
			
			WWW www = new WWW (url);
			
			StartCoroutine (WaitForRequest (www));

		} else {
			memo.text = "Missing the name of the country!";
		}


    }

	/// <summary>
	/// Gets the data wiki.
	/// </summary>
	/// <param name="country">Country.</param>
	public void GetDataWiki(string country)
	{

		CleanData ();

		Country = country;

		if (country != "") {
			string url = "https://en.wikipedia.org/wiki/" + country;

			memo.text += "url: " + url +"\r\n";
			Debug.Log (url);

			WWW www = new WWW (url);

			StartCoroutine (WaitForRequest (www));

		} else {
			memo.text = "Missing the name of the country!";
			Error = "Missing the name of the country!";
		}


	}

	/// <summary>
	/// Waits for request.
	/// </summary>
	/// <returns>The for request.</returns>
	/// <param name="www">Www.</param>
	private IEnumerator WaitForRequest(WWW www){
		yield return www;



		if (www.error == null) {
			Debug.Log ("WWW ok! "+ www.text.Length 	);


			HtmlDocument doc = new HtmlDocument();
			doc.LoadHtml(www.text);


			try{
			
				//Get the info geography table
				HtmlNode tabla=doc.DocumentNode.SelectSingleNode("//table[@class='infobox geography vcard']");
				//it can be null!

				#region Flag of the country
					//HtmlAttributeCollection atribu=	tabla.SelectSingleNode("//a[@title='Flag of "+inputtext.text+"']").FirstChild.Attributes;
					HtmlNode nodo2=tabla.SelectSingleNode("//a[contains(@title,'Flag of ')]");
				if (nodo2!=null) {
						HtmlNode child=nodo2.FirstChild;
						HtmlAttributeCollection atribu=child.Attributes;
						
						if (atribu!=null){
							foreach (HtmlAttribute atri in atribu)
							{
								if(atri.Name=="src"){
									Flag_URL=	atri.Value.ToString().Replace("//","");
								}
							}
						}
					}
				#endregion

				#region Official Language
					HtmlNodeCollection OffLanguage = tabla.SelectNodes("//th");

					HtmlNode Sibling=null;

					foreach(HtmlNode OfLangNodo in OffLanguage)
					{
						if (OfLangNodo.InnerText.ToString().Contains("Official lang"))
						{
							//get sibling node
							Sibling = OfLangNodo.NextSibling;
							break;
						}
					}

					//Sibling loop
					while (Sibling!=null)
					{
						Official_language+=" "+Sibling.InnerText.Replace("\n","");

						Sibling=Sibling.NextSibling;
					}

				Official_language=Official_language.Trim();
				#endregion


				#region National Language
				#endregion


				#region Population
					HtmlNode population=tabla.SelectSingleNode("//a[contains(text(),'Population')]");

				if (population!=null) {
						//GrandFather
						population= population.ParentNode.ParentNode;
						
						//second sibling
						population=population.NextSibling;
						
						//There are empty Sibling
						while ((population!=null) && (population.InnerText=="\n"))
						{
						population=population.NextSibling;
						}
						
						
						//add child info
						foreach(HtmlNode childs in population.ChildNodes)
						{
							Population+= childs.InnerText.Replace("\n","").Replace("&#160;"," ").Replace("•","").Trim()+" ";  // \n
						}
						
						
						Population=Population.Trim();
						
						//pending: remove [] and ()
						//add density
					}

				#endregion


				#region Ethnic groups
				//<a href="/wiki/Ethnic_groups" title="Ethnic groups" class="mw-redirect">Ethnic&nbsp;groups</a>
				HtmlNode nodoEtnichG=tabla.SelectSingleNode("//a[@title='Ethnic groups']");

				if (nodoEtnichG!=null) {
					//Father
					nodoEtnichG=nodoEtnichG.ParentNode;
					//GrandFather
					nodoEtnichG=nodoEtnichG.ParentNode;
					//Sibling
					nodoEtnichG=nodoEtnichG.NextSibling;
					
					while (nodoEtnichG!=null)
					{
					
						Ethnic_groups+=nodoEtnichG.InnerText.Replace("\n"," ")+" ";
					
						nodoEtnichG=nodoEtnichG.NextSibling;
					}
					
					Ethnic_groups=Ethnic_groups.Trim();
				}

				#endregion
				#region Anthem. There is a .ogg
					//<td class="anthem"
					HtmlNode AnthemNodo=tabla.SelectSingleNode("//td[@class='anthem']");

				if (AnthemNodo!=null){
					AnthemNodo=AnthemNodo.SelectSingleNode("//div[@class='mediaContainer']");
					if (AnthemNodo!=null){
						AnthemNodo=AnthemNodo.SelectSingleNode("//source");
						if (AnthemNodo!=null){
							Anthem=AnthemNodo.GetAttributeValue("src","").Replace("//","");
						
						}
					}
				}

				#endregion



			}catch(System.Exception exc) {
				
				Error = exc.Message;
				Debug.Log ("Error selectSingleNode(): "+exc.Message.ToString());
			}




		} else {
			Debug.Log ("WWW Error: "+www.error);
			Error = www.error;
		}

		//Launch the event
		DownloadedInfo (Error);
	
	}


	/// <summary>
	/// Gets the flag from of URL but it does not work jet
	/// </summary>
	public void GetFlag()
	{
		
		if (Flag_URL!="")
		{
//				WWW wwwflag2 = new WWW (Flag_URL);
//
//				// Wait for download to complete
//				yield  wwwflag2;
//
//				// assign texture
//
//				flag.material.SetTexture ("flag", wwwflag2.texture);
		}	


	}


	/// <summary>
	/// Exitapp this instance.
	/// </summary>
	public void exitapp()
	{
		Application.Quit ();
	}



}
