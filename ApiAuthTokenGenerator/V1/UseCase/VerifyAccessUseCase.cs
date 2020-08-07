using Amazon.Lambda.Core;
using ApiAuthTokenGenerator.V1.Boundary;
using ApiAuthTokenGenerator.V1.Gateways;
using ApiAuthTokenGenerator.V1.Helpers;
using ApiAuthTokenGenerator.V1.UseCase.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ApiAuthTokenGenerator.V1.UseCase
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
        public bool Execute(AuthorizerRequest authorizerRequest)
        {
            var validTokenClaims = ValidateTokenHelper.ValidateToken(authorizerRequest.Token);
            if (validTokenClaims != null && validTokenClaims.Count > 0)
            {
                var tokenId = validTokenClaims.Find(x => x.Type == "id").Value;
                var tokenData = _databaseGateway.GetTokenData(Int16.Parse(tokenId, CultureInfo.InvariantCulture));

                var apiName = _awsApiGateway.GetApiName(authorizerRequest.ApiAwsId);
                return VerifyAccessHelper.ShouldHaveAccess(authorizerRequest, tokenData, apiName);
            }
            return false;
        }
    }
}
