using RestSharp;
using VideoDown.Model;

namespace VideoDown.Utility
{
    public class Update
    {
        public static HttpReturnData CheckUpdate()
        {
            RestClient BaseClient = new RestClient($"{StaticClass.BaseUrl}/api/Update/UpdateInfos/UpdateName?name={System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}");
            var request = new RestRequest();
            var response = BaseClient.Get(request);
            var count = response.Content;
            var result = ProcessingData.JsonToHttpReturnData(response);
            return result;

        }
    }
}
