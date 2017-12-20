using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Text;
using System.Threading;

public class MobileCamera : MonoBehaviour
{
    private String _file;

    private WebCamTexture _webCamTexture;
    private WebCamTexture mWebcamTexBack;
    private WebCamTexture mWebcamTexFront;
    int back = -1, front = -1, current = -1;
    int nIdx = 0;
    //宽高比  
    //public float aspect = 4f / 3f;


    public RawImage rawImg_CamTexture;
    public Image img_Preview;

    public Text txtInfos;
    public Text txtInfos2;

    //public Button btn_Photo;
    //public Button btn_Change;

    void OnGUI()
    {

        if (GUI.Button(new Rect(10, 20, 200, 60), "捕获照片"))
        {
            StartCoroutine(GetTexture());
        }

        if (GUI.Button(new Rect(10, 100, 200, 60), "切换镜头"))
        {
            ChangeCamera();
        }
        if (GUI.Button(new Rect(10, 180, 200, 60), "录像"))
        {
            //录像  
            StartCoroutine(SeriousPhotoes());
        }
        if (GUI.Button(new Rect(10, 260, 200, 60), "停止"))
        {
            //停止捕获镜头
            if (_webCamTexture)
                _webCamTexture.Stop();
            StopAllCoroutines();
        }


        //if (mWebcamTexBack != null)
        //{
        //    // 捕获截图大小               —距X左屏距离   |   距Y上屏距离    
        //    GUI.DrawTexture(new Rect(50, Screen.height / 2 - 150, mWebcamTexBack.height * aspect, mWebcamTexBack.height), mWebcamTexBack, ScaleMode.ScaleToFit);
        //    string str = mWebcamTexBack.width + " - " + mWebcamTexBack.height;
        //    GUI.Label(new Rect(10, 260, 200, 60), str);
        //}


        PrintInfo();
    }

    // Use this for initialization
    void Start()
    {
        _file = Application.persistentDataPath + "/log.txt";
        Debug.Log(_file);

        rawImg_CamTexture.rectTransform.sizeDelta = new Vector2(Screen.width, Screen.height );
        //btn_Photo.onClick.AddListener(() => { StartCoroutine(GetTexture()); });
        //btn_Change.onClick.AddListener(() => {   ChangeCamera(); });
        StartCoroutine(InitAndOpenCamera());
        //PrintInfo();
    }

    //void Update()
    //{
    //    //if (Time.frameCount % 10 == 0)
    //    //{
    //    //    PrintInfo();
    //    //}

    //    //if (_webCamTexture)
    //    //{
    //    //    int videoRotationAngle = _webCamTexture.videoRotationAngle;
    //    //    rawImg_CamTexture.transform.rotation = Quaternion.Euler(0, 0, -videoRotationAngle);

    //    //}
    //}


    IEnumerator InitAndOpenCamera()
    {
        yield return StartCoroutine(ApplyCamera());
        OpenBackCam();
    }

    /// <summary>
    /// 申请相机
    /// </summary>
    /// <returns></returns>
    IEnumerator ApplyCamera()
    {
        if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);//授权
        }
        if (Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            int length = WebCamTexture.devices.Length;
            if (length <= 0)
            {
                WriteLog("你的设备没有摄像头！！！");
                enabled = false;
                yield break;
            }
            else if (length == 1)
            {
                mWebcamTexBack = new WebCamTexture(WebCamTexture.devices[0].name, Screen.width, Screen.height);
                //WebCamDevice device = WebCamTexture.devices[0]
            }
            else
            {
                for (int i = 0; i < length; i++)
                {
                    if (WebCamTexture.devices[i].isFrontFacing)
                    {
                        front = i;
                    }
                    else
                    {
                        if (back == -1)
                        {
                            back = i;
                        }
                    }
                }
                if (front == -1)
                {
                    front = back + 1;
                }
            }
        }
        //{
        //    enabled = false;
        //}
    }
    

    void OpenBackCam()
    {
        if (back == -1)
        {
            WriteLog("你的设备没有摄像头！！！");
            return;
        }

        current = back;
        if (mWebcamTexFront != null && mWebcamTexFront.isPlaying)
        {
            mWebcamTexFront.Stop();
        }

        mWebcamTexBack = new WebCamTexture(WebCamTexture.devices[current].name, Screen.width, Screen.height);
        mWebcamTexBack.Play();
        //rawImg_CamTexture.transform.rotation = Quaternion.Euler(0, 0, 0);
        rawImg_CamTexture.rectTransform.sizeDelta = new Vector2(Screen.width, (int)(mWebcamTexBack.width * Screen.width / mWebcamTexBack.height));
        rawImg_CamTexture.texture = mWebcamTexBack;
        nIdx = 0;
        rawImg_CamTexture.material.SetInt("_CameraIdx", nIdx);
    }

    void OpenFrontCam()
    {
        if (front == -1)
        {
            OpenBackCam();
            return;
        }

        current = front;
        if (mWebcamTexBack != null && mWebcamTexBack.isPlaying)
        {
            mWebcamTexBack.Stop();
        }

        mWebcamTexFront = new WebCamTexture(WebCamTexture.devices[current].name, Screen.width, Screen.height);
        mWebcamTexFront.Play();
        //rawImg_CamTexture.transform.rotation = Quaternion.Euler(0, 180, 0);
        rawImg_CamTexture.rectTransform.sizeDelta = new Vector2(Screen.width, (int)(mWebcamTexFront.width * Screen.width / mWebcamTexFront.height));
        rawImg_CamTexture.texture = mWebcamTexFront;
        nIdx = 1;
        rawImg_CamTexture.material.SetInt("_CameraIdx", nIdx);
    }

    public void ChangeCamera()
    {
        try
        {
            if (current == back)
            {
                OpenFrontCam();
            }
            else
            {
                OpenBackCam();
            }

            
    
        }
        catch (Exception err)
        {
            WriteLog(err);
            throw;
        }
       
    }

    public void GetCurrent_WebCamTexture()
    {
        if (current == back)
            _webCamTexture = mWebcamTexBack;
        else
            _webCamTexture = mWebcamTexFront;

        if (_webCamTexture)
            _webCamTexture.Pause();
    }

    public IEnumerator GetTexture()
    {
        GetCurrent_WebCamTexture();
        yield return new WaitForEndOfFrame();

        try
        {
            //// 旋转预览精灵
            //int videoRotationAngle = _webCamTexture.videoRotationAngle;
            //img_Preview.transform.parent.rotation = Quaternion.Euler(0, 0, -videoRotationAngle);

            //// 确定预览图，要截取镜头的大小
            //int w = _webCamTexture.width;
            //w = _webCamTexture.height > w ? w : _webCamTexture.height;
            //Vector2 offset = new Vector2((_webCamTexture.width - w) / 2, (_webCamTexture.height - w) / 2);

            Texture2D mTexture = new Texture2D(_webCamTexture.width, _webCamTexture.height, TextureFormat.ARGB32, false);
            mTexture.SetPixels(_webCamTexture.GetPixels(0, 0, _webCamTexture.width, _webCamTexture.height));

            //Texture2D mTexture = new Texture2D(_webCamTexture.height, _webCamTexture.width, TextureFormat.ARGB32, false);
            //mTexture.SetPixels(_webCamTexture.GetPixels(0, 0, _webCamTexture.width, _webCamTexture.height));

            mTexture.Apply();

            img_Preview.sprite = Sprite.Create(mTexture, new Rect(0, 0, mTexture.width, mTexture.height), Vector2.zero);
            img_Preview.material.SetInt("_CameraIdx", nIdx);
            _webCamTexture.Play();

            byte[] bt = mTexture.EncodeToPNG();
            string mPhotoPath = Application.persistentDataPath + "/" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".png";
            System.IO.File.WriteAllBytes(mPhotoPath, bt);
            WriteLog("照片路径  " + mPhotoPath);
            //if (callback != null) callback();      
        }
        catch (Exception err)
        {
            WriteLog(err);
            throw;
        }
    }

    /// <summary>  
    /// 连续捕获照片  
    /// </summary>  
    /// <returns>The photoes.</returns>  
    public IEnumerator SeriousPhotoes()
    {

        while (true)
        {
            yield return new WaitForEndOfFrame();
            try
            {
                Texture2D mTexture = new Texture2D(330, 440, TextureFormat.RGB24, true);
                mTexture.ReadPixels(new Rect(Screen.width / 2 - 165, Screen.height / 2 - 220, 330, 440), 0, 0, false);
                mTexture.Apply();
                print(mTexture);
                byte[] byt = mTexture.EncodeToPNG();
                System.IO.File.WriteAllBytes(Application.persistentDataPath + "/MulPhotoes/" + DateTime.Now.ToString("yyyyMMddHHmmss.fff") + ".png", byt);
                Thread.Sleep(50);
            }
            catch (Exception err)
            {
                WriteLog(err);
                throw;
            }
        }

    }

    void PrintInfo()
    {
        if (txtInfos == null || !txtInfos.isActiveAndEnabled) return;

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("screen: w = " + Screen.width + " h = " + Screen.height);
        sb.AppendLine("backCamText: " + mWebcamTexBack.width + ", " + mWebcamTexBack.height);
        sb.AppendLine("frontCamText: " + mWebcamTexFront.width + ", " + mWebcamTexFront.height);
        sb.AppendLine("RawImg: " + rawImg_CamTexture.rectTransform.sizeDelta.x + ", " + rawImg_CamTexture.rectTransform.sizeDelta.y);
        sb.Append("current: " + current);
        sb.Append("  back: " + back);
        sb.AppendLine("  front: " + front);
        sb.AppendLine("back videoRotationAngle:" + mWebcamTexBack.videoRotationAngle);
        sb.AppendLine("front videoRotationAngle:" + mWebcamTexFront.videoRotationAngle);
        txtInfos.text = sb.ToString();
    }


    void WriteLog(object str)
    {
        //string[] bytes = System.IO.File.ReadAllLines(_file);        
        // bytes[bytes.Length-1] += "\n\r"+str.ToString();
        // System.IO.File.WriteAllLines(_file, bytes);
        //for (int i = 0; i < bytes.Length; i++)
        //{
        //    txtInfos2.text += bytes[i] + "\n\r";
        //}

        byte[] bytes = System.IO.File.ReadAllBytes(_file);
        //bytes[bytes.Length - 1] += "\n\r" + str.ToString();
        //str.ToString().ToCharArray();
        byte[] addbytes = Encoding.Default.GetBytes(str.ToString().ToCharArray());
        byte[] totalbytes = new byte[bytes.Length + addbytes.Length];
        totalbytes.CopyTo(bytes, 0);
        totalbytes.CopyTo(addbytes, bytes.Length);
        System.IO.File.WriteAllBytes(_file, totalbytes);


    }
}