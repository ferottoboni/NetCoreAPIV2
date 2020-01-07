using junto_test_api.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace junto_test_api.Api.Test
{
    [TestClass]
    public class InfoControllerTest
    {
        [TestMethod]
        public void TestGetInfo_SouldReturnInfo()
        {
            var configuration = new Mock<IConfiguration>();
            var subject = new InfoController(configuration.Object);
            var response = ((ContentResult)subject.ApiInfo()).Content;
            Assert.IsTrue(response.Contains("NET.Core Api REST service started!"));
        }
    }
}
