using Amazon.Lambda.Core;
using ApiAuthVerifyToken.V1.Boundary;
using ApiAuthVerifyToken.V1.Gateways;
using ApiAuthVerifyToken.V1.Helpers;
using ApiAuthVerifyToken.V1.UseCase.Interfaces;

namespace ApiAuthVerifyToken.V1.UseCase
{
    public class VerifyAccessUseCase : IVerifyAccessUseCase
    {
        private IAuthTokenDatabaseGateway _databaseGateway;
        private IAwsApiGateway _awsApiGateway;
        public VerifyAccessUseCase(IAuthTokenDatabaseGateway databaseGateway, IAwsApiGateway awsApiGateway)
        {
            _databaseGateway = databaseGateway;
            _awsApiGateway = awsApiGateway;
        }
        public AccessDetails Execute(AuthorizerRequest authorizerRequest)
        {
            var validTokenClaims = ValidateTokenHelper.ValidateToken(authorizerRequest.Token);
            if (validTokenClaims == null || validTokenClaims.Count == 0) return ReturnNotAuthorised(authorizerRequest);

            var tokenId = validTokenClaims.Find(x => x.Type == "id").Value;
            if (!int.TryParse(tokenId, out int id)) return ReturnNotAuthorised(authorizerRequest);

            var tokenData = _databaseGateway.GetTokenData(id);
            var apiName = _awsApiGateway.GetApiName(authorizerRequest.ApiAwsId);
            LambdaLogger.Log($"API name retrieved - {apiName}");
            return new AccessDetails
            {
                Allow = VerifyAccessHelper.ShouldHaveAccess(authorizerRequest, tokenData, apiName),
                User = $"{tokenData.ConsumerName}{tokenData.Id}"
            };
        }

        private static AccessDetails ReturnNotAuthorised(AuthorizerRequest authorizerRequest)
        {
            LambdaLogger.Log(
                $"Token beginning with {authorizerRequest.Token.Substring(0, 8)} is invalid or should not have access to" +
                $" {authorizerRequest.ApiAwsId} - {authorizerRequest.ApiEndpointName}" +
                $" in {authorizerRequest.Environment}");
            return new AccessDetails { Allow = false, User = "user" };
        }
    }

    public class AccessDetails
    {
        public bool Allow { get; set; }
        public string User { get; set; }
    }
}
