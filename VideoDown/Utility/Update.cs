using VideoDown.Model;

namespace VideoDown.Utility
{
    public class Update
    {
        public static HttpReturnData CheckUpdate()
        {
            Http http = new Http();
            var result = http.Http_Get_301($"{StaticClass.BaseUrl}/api/Soft/UpdateInfo/UpdateName?name={System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}");

            return ProcessingData.JsonToHttpReturnData(result);
        }
    }
}