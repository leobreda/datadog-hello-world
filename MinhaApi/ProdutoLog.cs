using System.Net;
using System.Text;

namespace MinhaApi
{
    public class ProdutoLog
    {
        private static readonly HttpClient client = new HttpClient();

        private Dictionary<string, object> payload { get; set; }

        public ProdutoLog(HttpRequest request)
        {
            payload = new Dictionary<string, object>();
            Dictionary<string, object> dHeaders = new Dictionary<string, object>();
            dHeaders.Add("client_id", request.Headers["client_id"]);
            dHeaders.Add("correlation_id", request.Headers["correlation_id"]);
            dHeaders.Add("method", request.Method);

            if (!(string.IsNullOrEmpty(request.Headers["latitude"]) || string.IsNullOrEmpty(request.Headers["longitude"])))
            {
                dHeaders.Add("latitude", Convert.ToDecimal(request.Headers["latitude"].ToString().Replace(".", ",")));
                dHeaders.Add("longitude", Convert.ToDecimal(request.Headers["longitude"].ToString().Replace(".", ",")));
            }
            payload.Add("headers", dHeaders);
        }


        public void Get(int id, Produto produto, HttpStatusCode status_code)
        {
            payload.Add("request", id);
            payload.Add("response", produto);
            payload.Add("status_code", status_code);
            Log();
        }

        public void Get(List<Produto> list, HttpStatusCode status_code)
        {
            payload.Add("response", list);
            payload.Add("status_code", status_code);
            Log();
        }

        public void Post(Produto produto, HttpStatusCode status_code, Exception ex = null)
        {
            payload.Add("request", produto);
            payload.Add("status_code", status_code);

            if (ex is not null)
                payload.Add("exception", GetException(ex));

            Log();
        }

        public void Put(Produto antes, Produto depois, HttpStatusCode status_code, Exception ex = null)
        {
            payload.Add("request", antes);
            payload.Add("response", depois);

            payload.Add("status_code", status_code);

            if (ex is not null)
                payload.Add("exception", GetException(ex));

            Log();
        }

        public void Delete(int id, Produto produto, HttpStatusCode status_code, Exception ex = null)
        {
            payload.Add("request", produto);
            payload.Add("status_code", status_code);

            if (ex is not null)
                payload.Add("exception", GetException(ex));

            Log();
        }



        private Dictionary<string, string> GetException(Exception ex)
        {
            Dictionary<string, string> dException = new Dictionary<string, string>
            {
                { "message", ex.Message },
                { "stack_trace", ex.StackTrace }
            };

            return dException;
        }

        private void Log()
        {
            /// Relação completa de TAGs em:
            /// https://docs.datadoghq.com/getting_started/tagging/
            //payload.Add("ddtags", "env:production,host:SRV010AWS,source:api,service:Produtos");
            //payload.Add("status", Status());

            string json = System.Text.Json.JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");



            //todo remover result, while, exception

            Task result = client.PostAsync("https://http-intake.logs.datadoghq.com/v1/input/" + Environment.GetEnvironmentVariable("DD-API-KEY"),
                content);

            while (!result.IsCompleted) { }

            if (result.Exception is not null)
                throw result.Exception;

        }

        private string Status()
        {
            switch ((HttpStatusCode)payload["status_code"])
            {
                case HttpStatusCode.InternalServerError:
                    return "error";

                case HttpStatusCode.NotFound:
                    return "warn";

                default:
                    return "info";
            }
        }
    }
}
