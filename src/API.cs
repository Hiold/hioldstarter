using System.IO;
using System;
using System.Reflection;
using System.Collections.Generic;
using HioldMod.src.UserTools;
using System.Net;

namespace HioldMod
{
    public class API : IModApi
    {
        public static string AssemblyPath = string.Format("{0}/", getModDir());
        private static Assembly MainAssembly;
        private static IModApi ApiInstance;
        public void InitMod()
        {
            string version = HttpTools.HttpPost("http://qc.hiold.net:8081/hioldapi/version.json");
            Log.Out("[HIOLD] 远程version信息：{0}", version);
            Dictionary<string, string> versionRemote = HttpTools.stringToDic(version);
            //是否执行更新
            bool modhasUpdate = false;
            bool apihasUpdate = false;
            //读取本地文件
            string txt = "";
            try
            {
                StreamReader sr = new StreamReader(@AssemblyPath + "version.json");
                while (!sr.EndOfStream)
                {
                    string str = sr.ReadLine();
                    txt += str;
                }
                sr.Close();
            }
            catch (Exception e)
            {
                Log.Out("[HIOLD] 没有发现version信息，执行下载");
            }
            //校验本地文件
            //没有检测到文件，写入新文件 并执行更新
            if (txt.Equals("") || txt == null)
            {
                modhasUpdate = true;
                apihasUpdate = true;
                //写入文件
                using (StreamWriter sw = new StreamWriter(@AssemblyPath + "version.json", true))
                {
                    sw.WriteLine(version);
                    sw.Flush();
                    sw.Close();
                }
            }

            //没有获取到更新信息不执行更新
            if (versionRemote.Count <= 0)
            {
                modhasUpdate = false;
                apihasUpdate = false;
            }

            Dictionary<string, string> versionLocal = HttpTools.stringToDic(txt);
            //判断版本情况
            if (versionRemote.Count > 0 && versionLocal.Count > 0)
            {
                //
                if (versionRemote.TryGetValue("modversion", out string remoteModVersion))
                {
                    if (versionLocal.TryGetValue("modversion", out string localModVersion))
                    {
                        if (remoteModVersion.Equals(localModVersion))
                        {
                            modhasUpdate = false;
                        }
                        else
                        {
                            modhasUpdate = true;
                        }
                    }
                }

                if (versionRemote.TryGetValue("apiversion", out string remoteAPIVersion))
                {
                    if (versionLocal.TryGetValue("apiversion", out string localAPIVersion))
                    {
                        if (remoteAPIVersion.Equals(localAPIVersion))
                        {
                            apihasUpdate = false;
                        }
                        else
                        {
                            apihasUpdate = true;
                        }
                    }
                }
            }
            //下载dll
            if (modhasUpdate)
            {
                Log.Out("[HIOLD] 检测到更新mod更新，正在下载");
                //覆盖旧文件
                if (DownloadFile("http://qc.hiold.net:8081/hioldapi/main.bin", AssemblyPath + "main.bin.new"))
                {
                    File.Delete(AssemblyPath + "main.bin");
                    File.Copy(AssemblyPath + "main.bin.new", AssemblyPath + "main.bin");
                    File.Delete(AssemblyPath + "main.bin.new");
                }
            }
            else
            {
                Log.Out("[HIOLD] mod已为最新，不再更新");
            }
            //下载api
            if (apihasUpdate)
            {
                Log.Out("[HIOLD] 检测到更新api更新，正在下载");
                if (DownloadFile("http://qc.hiold.net:8081/hioldapi/api.bin", AssemblyPath + "api.bin.new"))
                {
                    File.Delete(AssemblyPath + "api.bin");
                    File.Copy(AssemblyPath + "api.bin.new", AssemblyPath + "api.bin");
                    File.Delete(AssemblyPath + "api.bin.new");
                }
            }
            else
            {
                Log.Out("[HIOLD] api已为最新，不再更新");
            }





            //赋值主文件进行加载
            File.Delete(AssemblyPath + "main.dlc");
            File.Copy(AssemblyPath + "main.bin", AssemblyPath + "main.dlc");
            //反射获取数据
            LoadAssembly(AssemblyPath + "main.dlc");
        }

        //加载mod
        public static void LoadAssembly(string _path)
        {
            Log.Out("[HIOLD] 正在载入MOD , 创建实例");
            Type typeFromHandle = typeof(IModApi);
            MainAssembly = Assembly.LoadFrom(_path);
            foreach (Type type in MainAssembly.GetTypes())
            {
                if (typeFromHandle.IsAssignableFrom(type))
                {
                    //Log.Out("[MODS] Found ModAPI in " + System.IO.Path.GetFileName(keyValuePair.Key) + ", creating instance");
                    ApiInstance = (Activator.CreateInstance(type) as IModApi);
                    //return true;
                }
            }
            //执行初始化
            ApiInstance.InitMod();
        }

        public static string getModDir()
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }


        private static bool DownloadFile(string URL, string filename)
        {
            try
            {
                HttpWebRequest Myrq = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(URL);
                HttpWebResponse myrp = (System.Net.HttpWebResponse)Myrq.GetResponse();
                Stream st = myrp.GetResponseStream();
                Stream so = new System.IO.FileStream(filename, System.IO.FileMode.Create);
                byte[] by = new byte[1024];
                int osize = st.Read(by, 0, (int)by.Length);
                while (osize > 0)
                {
                    so.Write(by, 0, osize);
                    osize = st.Read(by, 0, (int)by.Length);
                }
                so.Close();
                st.Close();
                myrp.Close();
                Myrq.Abort();
                return true;
            }
            catch (System.Exception e)
            {
                return false;
            }
        }

    }
}