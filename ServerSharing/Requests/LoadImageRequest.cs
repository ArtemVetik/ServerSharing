using Ydb.Sdk.Table;
using System.Text;
using Ydb.Sdk.Value;
using ServerSharing.Data;

namespace ServerSharing
{
    public class LoadImageRequest : BaseRequest
    {
        public LoadImageRequest(TableClient tableClient, Request request)
            : base(tableClient, request)
        { }

        protected async override Task<Response> Handle(TableClient client, Request request)
        {
            var response = await client.SessionExec(async session =>
            {
                var query = $@"
                    DECLARE $id AS string;

                    SELECT image
                    FROM `{Tables.Images}`
                    WHERE id = $id;
                ";

                return await session.ExecuteDataQuery(
                    query: query,
                    txControl: TxControl.BeginSerializableRW().Commit(),
                    parameters: new Dictionary<string, YdbValue>
                    {
                        { "$id", YdbValue.MakeString(Encoding.UTF8.GetBytes(request.body)) }
                    }
                );
            });

            if (response.Status.IsSuccess == false)
                return new Response((uint)response.Status.StatusCode, response.Status.StatusCode.ToString(), string.Empty);

            var queryResponse = (ExecuteDataQueryResponse)response;

            if (queryResponse.Result.ResultSets[0].Rows.Count == 0)
                throw new InvalidOperationException("Record not found!");

            var data = queryResponse.Result.ResultSets[0].Rows[0]["image"].GetOptionalString();

            return new Response((uint)response.Status.StatusCode, response.Status.StatusCode.ToString(), Convert.ToBase64String(data));
        }
    }
}