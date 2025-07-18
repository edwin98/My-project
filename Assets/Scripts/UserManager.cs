using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserManager : MonoBehaviour
{
    public static UserManager u_m;
    public Button addButton;
    public Button delAllButton;
    public Button homeButton;
    public Button endButton;
    public Button previousButton;
    public Button nextPageButton;
    public Button importButton;
    public Button exportButton;
    public Dropdown role; 
    public InputField keyWord;
    public Dropdown post;
    public Dropdown grade;
    [System.NonSerialized]
    public int itemCount = 15;
    // [System.NonSerialized]
    // public string[] ids ;
    [System.NonSerialized]
    public List<string> userIds = new List<string>();
    //[System.NonSerialized]
    //public List<User> userList = new List<User>();//查询所有用户数据(导出)
    [System.NonSerialized]
    public bool refresh = false;


    //public Button detailButton;
    //public Button delButton;

    private string type = "";
    private string tipType = "";
    private string modifyContent = "";

    private void Awake()
    {
        u_m = this;

    }

    //Start is called before the first frame update
    void Start()
    {

        //role.ClearOptions();
        //List<string> options = new List<string>();
        //options.Add("全部");
        //for (int i = 0; i < DataBase.Instance().roleList.Count; i++)
        //{
        //    options.Add(DataBase.Instance().roleList[i].RoleName);
        //}
        //role.AddOptions(options);
    }

    // Update is called once per frame
    void Update()
    {
        type = "";
        tipType = "";
        modifyContent = "";
    }

   


    public void ShowTips(string type)
    {
        if (userIds.Count > 0)
        {
            if ("Del".Equals(type))
            {
               // UIManager.Instance().OpenTips(Common.deleteTip, Common.userDelete);
               //弹出提示信息
            }
           
        }
        else
        {
            if ("Del".Equals(type))
            {
                //UIManager.Instance().OpenTips(Common.errorResultTip, Common.delErrorResult);
                //弹出提示信息
            }

        }


    }

    public void ShowImportTips()
    {
        //UIManager.Instance().OpenTips(Common.importTip, Common.userImport);
    }

    public void ShowExportTips()
    {
        //UIManager.Instance().OpenTips(Common.exportTip, Common.userExport);
    }

    /// <summary>
    /// 导出Excel表
    /// </summary>
    public void ExportUsers()
    {
        //if (userList.Count <= 0) return;
        //WriteExcelTool.WriteUserExcel(new List<string>() { "序号", "角色类型", "账号/学员学号", "姓名",  "性别", "民族", "身份证号", "出生日期", "籍贯", "所属单位", "政治面貌", "现居住地", "联系电话", "电子邮箱", "备注" }, userList);
    }

    /// <summary>
    /// 批量导入用户
    /// </summary>
    public void ImportUsers(string path)
    {
        //List<User> users = new List<User>();
        //ReadExcelTool excelReadAnd = new ReadExcelTool();
        //if (string.IsNullOrEmpty(path)) return;
        //users.AddRange(excelReadAnd.ReadUserInfoConfig(path));
        //if (users.Count > 0)
        //{
        //    string temp = ServiceManager.Instance().BatchAddUsers(users);
        //    if ("".Equals(temp))
        //    {
        //        UserManager.u_m.refresh = true;
        //        Log logInfo = new Log();
        //        logInfo.UserId = DataBase.Instance().loginUserInfo.UserId;
        //        logInfo.Ip = DataBase.Instance().loginUserIp;
        //        logInfo.Content = Common.userInfoImportLog;
        //        ServiceManager.Instance().AddLog(logInfo);
        //        type = Common.successResultTip;
        //    }
        //    else
        //    {
        //        type = Common.errorResultTip;
        //        tipType = temp;
        //    }
        //}
    }

    /// <summary>
    /// 批量删除
    /// </summary>
    public void BatchDeleteUsers()
    {
        //int temp = ServiceManager.Instance().DeleteUsers(userIds);
        int status = 1;
        //if (temp == 0)
        if(true)
        {
            status = 0;
            UserManager.u_m.refresh = true;
            //type = Common.successResultTip;
            tipType = "批量删除成功！";
        }
        else
        {
            //type = Common.errorResultTip;
            tipType = "批量删除失败！";
        }
        //增加log
        //LogInfo logInfo = new LogInfo();
        //logInfo.UserName = DataBase.Instance().loginUserInfo.UserName;
        //logInfo.OperationStatus = status.ToString();
        //logInfo.OperationName = Common.userDeleteLog;
        //ServiceManager.Instance().AddLog(logInfo);

    }


    public void EmpowerUsers(string type)
    {
        //    List<User> userList = new List<User>();
        //    for (int i = 0; i < userIds.Count; i ++)
        //    {
        //        User user = ServiceManager.Instance().GetUserInfo(userIds[i]);
        //        user.Type = type;
        //        int temp = ServiceManager.Instance().ModifyUser(user);
        //        if (temp == 0)
        //        {
        //            UserManager.u_m.refresh = true;
        //            Log logInfo = new Log();
        //            logInfo.UserId = DataBase.Instance().loginUserInfo.UserId;
        //            logInfo.Ip = DataBase.Instance().loginUserIp;
        //            if ("0".Equals(type))
        //            {
        //                logInfo.Content = Common.userNoEmpowerLog;
        //            }
        //            else
        //            {
        //                logInfo.Content = Common.userEmpowerLog;
        //            }
        //            ServiceManager.Instance().AddLog(logInfo);
        //            //type = Common.successResultTip;
        //            tipType = "批量授权成功！";
        //        }
        //        else
        //        {

        //            //type = Common.errorResultTip;
        //            tipType = "批量授权失败！";
        //        }
        //    }


        //}

    }

}