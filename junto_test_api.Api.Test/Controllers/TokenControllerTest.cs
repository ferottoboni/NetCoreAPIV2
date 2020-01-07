using junto_test_api.Api.Controllers;
using junto_test_api.Api.Models;
using junto_test_api.Domain;
using junto_test_api.Domain.Service;
using junto_test_api.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace junto_test_api.Api.Test
{
    [TestClass]
    public class TokenControllerTest
    {
        [TestMethod]
        public void TestCreateWithInvalidIntegrationKey_ShouldReturnUnauthorized()
        {
            var tokenService = new Mock<ITokenService<TokenViewModel, Token>>();
            TokenViewModel tokenViewModel = null;
            tokenService.Setup(s => s.CreateNewToken(It.IsAny<CreateTokenViewModel>())).Returns(tokenViewModel);
            var subject = new TokenController(tokenService.Object);
            var response = subject.Create(new Models.CreateTokenModel());
            Assert.AreEqual((new UnauthorizedResult()).GetType(), response.GetType());
        }

        [TestMethod]
        public void TestCreateWithCorrectIntegrationKey_ShouldReturnOk()
        {
            var tokenService = new Mock<ITokenService<TokenViewModel, Token>>();
            var dueDate = DateTime.Now.AddDays(1);
            var id = 123;
            var key = "NEW_KEY";
            TokenViewModel tokenViewModel = new TokenViewModel()
            {
                DueDate = dueDate,
                Id = id,
                Key = key
            };
            tokenService.Setup(s => s.CreateNewToken(It.IsAny<CreateTokenViewModel>())).Returns(tokenViewModel);
            var subject = new TokenController(tokenService.Object);
            var response = subject.Create(new Models.CreateTokenModel());
            Assert.AreEqual((new OkObjectResult(tokenViewModel)).GetType(), response.GetType());
            var responseValue = ((TokenModel)((OkObjectResult)response).Value);
            Assert.AreEqual(dueDate, responseValue.DueDate);
            Assert.AreEqual(key, responseValue.Key);
        }
    }
}
