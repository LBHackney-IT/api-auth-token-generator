using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using ApiAuthTokenGenerator.V1.Boundary;
using ApiAuthTokenGenerator.V1.UseCase.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace ApiAuthTokenGenerator
{
    public class LambdaHandler
    {
        private readonly IServiceProvider _serviceProvider;
        public LambdaHandler()
        {
            var services = new ServiceCollection();
            services.Configure();
            _serviceProvider = services.BuildServiceProvider();
        }

        public LambdaHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public APIGatewayCustomAuthorizerResponse VerifyToken(APIGatewayCustomAuthorizerRequest request)
        {
            var authorizerRequest = new AuthorizerRequest
            {
                ApiEndpointName = request.RequestContext.ResourcePath,
                ApiAwsId = request.RequestContext.ApiId,
                Environment = request.RequestContext.Stage,
                Token = request.Headers["Authorization"]
            };
            var verifyAccessUseCase = _serviceProvider.GetService<IVerifyAccessUseCase>();

            var result = verifyAccessUseCase.Execute(authorizerRequest);

            return new APIGatewayCustomAuthorizerResponse
            {
                PolicyDocument = new APIGatewayCustomAuthorizerPolicy
                {
                    Version = "2012-10-17",
                    Statement = new List<APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement>() {
                      new APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement
                      {
                           Action = new HashSet<string>(){"execute-api:Invoke"},
                           Effect = result ? "Allow" : "Deny",
                           Resource = new HashSet<string>(){  request.MethodArn } // resource arn here
                      }
                },
                }
            };

        }
    }
}