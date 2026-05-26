using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;

#if UNITY_IOS
using UnityEditor.iOS.Xcode;

public class iOSPostProcessBuild
{
    [PostProcessBuild(1)]
    public static void OnPostProcessBuild(BuildTarget target, string buildPath)
    {
        if (target != BuildTarget.iOS) return;

        string plistPath = Path.Combine(buildPath, "Info.plist");
        PlistDocument plist = new PlistDocument();
        plist.ReadFromFile(plistPath);

        PlistElementDict root = plist.root;

        // GoogleService-Info.plist에서 REVERSED_CLIENT_ID 읽기
        string googlePlistPath = Path.Combine(Application.dataPath, "GoogleService-Info.plist");
        if (File.Exists(googlePlistPath))
        {
            PlistDocument googlePlist = new PlistDocument();
            googlePlist.ReadFromFile(googlePlistPath);

            string reversedClientId = googlePlist.root["REVERSED_CLIENT_ID"]?.AsString();
            if (!string.IsNullOrEmpty(reversedClientId))
            {
                // URL Types 배열 가져오기 (없으면 생성)
                PlistElementArray urlTypes = root.values.ContainsKey("CFBundleURLTypes")
                    ? root["CFBundleURLTypes"].AsArray()
                    : root.CreateArray("CFBundleURLTypes");

                // 이미 등록된 경우 중복 방지
                bool alreadyAdded = false;
                foreach (var item in urlTypes.values)
                {
                    var dict = item.AsDict();
                    if (dict != null && dict.values.ContainsKey("CFBundleURLSchemes"))
                    {
                        foreach (var scheme in dict["CFBundleURLSchemes"].AsArray().values)
                        {
                            if (scheme.AsString() == reversedClientId)
                            {
                                alreadyAdded = true;
                                break;
                            }
                        }
                    }
                }

                if (!alreadyAdded)
                {
                    PlistElementDict urlSchemeDict = urlTypes.AddDict();
                    urlSchemeDict.SetString("CFBundleURLName", "Google Sign-In");
                    PlistElementArray schemes = urlSchemeDict.CreateArray("CFBundleURLSchemes");
                    schemes.AddString(reversedClientId);
                    Debug.Log($"[iOSPostProcess] Google Sign-In URL Scheme 등록 완료: {reversedClientId}");
                }
                else
                {
                    Debug.Log("[iOSPostProcess] Google Sign-In URL Scheme 이미 등록됨");
                }
            }
            else
            {
                Debug.LogWarning("[iOSPostProcess] REVERSED_CLIENT_ID 값을 찾을 수 없습니다.");
            }
        }
        else
        {
            Debug.LogWarning("[iOSPostProcess] GoogleService-Info.plist 파일을 찾을 수 없습니다. Assets/ 폴더에 추가하세요.");
        }

        plist.WriteToFile(plistPath);
    }
}
#endif
