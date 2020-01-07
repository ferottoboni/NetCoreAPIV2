using junto_test_api.Api.Controllers;
using junto_test_api.Domain;
using junto_test_api.Domain.Service;
using junto_test_api.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Linq;

namespace junto_test_api.Api.Test
{
    [TestClass]
    public class UserControllerTest
    {
        [TestMethod]
        public void TestGetAllToValidateTokenWithInvalidToken_ShouldReturnUnauthorized()
        {
            #region Mock
            var tokenService = new Mock<ITokenService<TokenViewModel, Token>>();
            tokenService.Setup(s => s.ValidateToken(It.IsAny<string>())).Returns(false);
            var userService = new Mock<IUserService<UserViewModel, User>>();
            #endregion

            var subject = new UserController(userService.Object, tokenService.Object);
            var response = subject.GetAll("NEW_TOKEN");
            Assert.AreEqual((new UnauthorizedResult()).GetType(), response.GetType());
        }

        [TestMethod]
        public void TestGetAll_ShouldReturn2RecordsMocked()
        {
            #region Mock
            var tokenService = new Mock<ITokenService<TokenViewModel, Token>>();
            tokenService.Setup(s => s.ValidateToken(It.IsAny<string>())).Returns(true);
            List<UserViewModel> mockedReturn = buildUserViewModelList();
            var userService = new Mock<IUserService<UserViewModel, User>>();
            userService.Setup(s => s.GetAll()).Returns(mockedReturn);
            #endregion

            var subject = new UserController(userService.Object, tokenService.Object);
            var response = subject.GetAll("NEW_TOKEN");
            var responseValue = ((IEnumerable<UserViewModel>)((OkObjectResult)response).Value);
            Assert.AreEqual(2, responseValue.Count());
            Assert.AreEqual("James Hetfield", responseValue.First().Name);
            Assert.AreEqual("Lars Ulrich", responseValue.Last().Name);
        }

        [TestMethod]
        public void TestGetById_ShouldReturnTheCorrectRecord()
        {
            #region Mock
            var tokenService = new Mock<ITokenService<TokenViewModel, Token>>();
            tokenService.Setup(s => s.ValidateToken(It.IsAny<string>())).Returns(true);
            List<UserViewModel> mockedReturn = buildUserViewModelList();
            var userService = new Mock<IUserService<UserViewModel, User>>();
            userService.Setup(s => s.GetOne(It.IsAny<int>())).Returns(mockedReturn.Last());
            #endregion

            var subject = new UserController(userService.Object, tokenService.Object);
            var response = subject.GetById(2, "NEW_TOKEN");
            var responseValue = ((UserViewModel)((OkObjectResult)response).Value);
            Assert.AreEqual(2, responseValue.Id);
            Assert.AreEqual("Lars Ulrich", responseValue.Name);
            Assert.AreEqual("lars.ulrich@algumprovedordeemail.com", responseValue.Email);
        }

        [TestMethod]
        public void TestCreateWithModelNull_ShouldReturnBadRequest()
        {
            #region Mock
            var tokenService = new Mock<ITokenService<TokenViewModel, Token>>();
            var userService = new Mock<IUserService<UserViewModel, User>>();
            #endregion

            var subject = new UserController(userService.Object, tokenService.Object);
            var response = subject.Create(null, "SOMETHING");
            Assert.AreEqual((new BadRequestResult()).GetType(), response.GetType());
        }

        [TestMethod]
        public void TestCreateWithCorrectModel_ShouldCreateAndReturnCreatedResult()
        {
            #region Mock
            var tokenService = new Mock<ITokenService<TokenViewModel, Token>>();
            tokenService.Setup(s => s.ValidateToken(It.IsAny<string>())).Returns(true);
            List<UserViewModel> mockedReturn = buildUserViewModelList();
            var userService = new Mock<IUserService<UserViewModel, User>>();
            userService.Setup(s => s.Add(It.IsAny<UserViewModel>())).Returns(2342);
            #endregion

            var userViewModel = BuildUserViewModel();
            var subject = new UserController(userService.Object, tokenService.Object);
            var response = subject.Create(userViewModel, "SOMETHING");
            Assert.AreEqual((new CreatedResult($"api/User/{userViewModel.Id}", userViewModel.Id)).GetType(), response.GetType());
            var responseValue = ((int)((CreatedResult)response).Value);
            Assert.AreEqual(2342, responseValue);

        }

        [TestMethod]
        public void TestUpdateWithModelNull_ShouldReturnBadRequest()
        {
            #region Mock
            var tokenService = new Mock<ITokenService<TokenViewModel, Token>>();
            var userService = new Mock<IUserService<UserViewModel, User>>();
            #endregion

            var subject = new UserController(userService.Object, tokenService.Object);
            var response = subject.Update(1, null, "SOMETHING");
            Assert.AreEqual((new BadRequestResult()).GetType(), response.GetType());
        }

        [TestMethod]
        public void TestUpdateWithServiceErrorZero_ShouldReturnStatusCodeNotModified()
        {
            #region Mock
            var tokenService = new Mock<ITokenService<TokenViewModel, Token>>();
            tokenService.Setup(s => s.ValidateToken(It.IsAny<string>())).Returns(true);
            List<UserViewModel> mockedReturn = buildUserViewModelList();
            var userService = new Mock<IUserService<UserViewModel, User>>();
            userService.Setup(s => s.Update(It.IsAny<UserViewModel>())).Returns(0);
            #endregion

            var userViewModel = BuildUserViewModel();
            var subject = new UserController(userService.Object, tokenService.Object);
            var response = subject.Update(userViewModel.Id, userViewModel, "SOMETHING");
            Assert.AreEqual((new StatusCodeResult(304)).GetType(), response.GetType());
            Assert.AreEqual(304, ((StatusCodeResult)response).StatusCode);
        }

        [TestMethod]
        public void TestUpdateWithServiceErrorMinusOne_ShouldReturnStatusCode412()
        {
            #region Mock
            var tokenService = new Mock<ITokenService<TokenViewModel, Token>>();
            tokenService.Setup(s => s.ValidateToken(It.IsAny<string>())).Returns(true);
            List<UserViewModel> mockedReturn = buildUserViewModelList();
            var userService = new Mock<IUserService<UserViewModel, User>>();
            userService.Setup(s => s.Update(It.IsAny<UserViewModel>())).Returns(-1);
            #endregion

            var userViewModel = BuildUserViewModel();
            var subject = new UserController(userService.Object, tokenService.Object);
            var response = subject.Update(userViewModel.Id, userViewModel, "SOMETHING");
            Assert.AreEqual((new ObjectResult(412)).GetType(), response.GetType());
            Assert.AreEqual(412, ((ObjectResult)response).StatusCode);
        }

        [TestMethod]
        public void TestUpdateWithServiceResponseOk_ShouldReturnAcceptedResponse()
        {
            #region Mock
            var tokenService = new Mock<ITokenService<TokenViewModel, Token>>();
            tokenService.Setup(s => s.ValidateToken(It.IsAny<string>())).Returns(true);
            List<UserViewModel> mockedReturn = buildUserViewModelList();
            var userService = new Mock<IUserService<UserViewModel, User>>();
            userService.Setup(s => s.Update(It.IsAny<UserViewModel>())).Returns(262018);
            #endregion

            var userViewModel = BuildUserViewModel();
            var subject = new UserController(userService.Object, tokenService.Object);
            var response = subject.Update(userViewModel.Id, userViewModel, "SOMETHING");
            Assert.AreEqual((new AcceptedResult()).GetType(), response.GetType());
        }

        [TestMethod]
        public void TestDeleteWithServiceErrorZero_ShouldReturnStatusCodeNotModified()
        {
            #region Mock
            var tokenService = new Mock<ITokenService<TokenViewModel, Token>>();
            tokenService.Setup(s => s.ValidateToken(It.IsAny<string>())).Returns(true);
            List<UserViewModel> mockedReturn = buildUserViewModelList();
            var userService = new Mock<IUserService<UserViewModel, User>>();
            userService.Setup(s => s.Remove(It.IsAny<int>())).Returns(0);
            #endregion

            var userViewModel = BuildUserViewModel();
            var subject = new UserController(userService.Object, tokenService.Object);
            var response = subject.Delete(userViewModel.Id, "SOMETHING");
            Assert.AreEqual((new NotFoundResult()).GetType(), response.GetType());
        }

        [TestMethod]
        public void TestDeleteWithServiceErrorMinusOne_ShouldReturnStatusCode412()
        {
            #region Mock
            var tokenService = new Mock<ITokenService<TokenViewModel, Token>>();
            tokenService.Setup(s => s.ValidateToken(It.IsAny<string>())).Returns(true);
            List<UserViewModel> mockedReturn = buildUserViewModelList();
            var userService = new Mock<IUserService<UserViewModel, User>>();
            userService.Setup(s => s.Remove(It.IsAny<int>())).Returns(-1);
            #endregion

            var userViewModel = BuildUserViewModel();
            var subject = new UserController(userService.Object, tokenService.Object);
            var response = subject.Delete(userViewModel.Id, "SOMETHING");
            Assert.AreEqual((new ObjectResult(412)).GetType(), response.GetType());
            Assert.AreEqual(412, ((ObjectResult)response).StatusCode);
        }

        [TestMethod]
        public void TestDeleteWithServiceResponseOk_ShouldReturnOkResponse()
        {
            #region Mock
            var tokenService = new Mock<ITokenService<TokenViewModel, Token>>();
            tokenService.Setup(s => s.ValidateToken(It.IsAny<string>())).Returns(true);
            List<UserViewModel> mockedReturn = buildUserViewModelList();
            var userService = new Mock<IUserService<UserViewModel, User>>();
            userService.Setup(s => s.Remove(It.IsAny<int>())).Returns(123);
            #endregion

            var userViewModel = BuildUserViewModel();
            var subject = new UserController(userService.Object, tokenService.Object);
            var response = subject.Delete(userViewModel.Id, "SOMETHING");
            Assert.AreEqual((new OkResult()).GetType(), response.GetType());
        }

        [TestMethod]
        public void TestChangePasswordWithInvalidUser_ShouldReturnBadRequest()
        {
            #region Mock
            var tokenService = new Mock<ITokenService<TokenViewModel, Token>>();
            tokenService.Setup(s => s.ValidateToken(It.IsAny<string>())).Returns(true);
            List<UserViewModel> mockedReturn = buildUserViewModelList();
            var userService = new Mock<IUserService<UserViewModel, User>>();
            userService.Setup(s => s.ChangePassword(It.IsAny<ChangePasswordViewModel>())).Returns(false);
            #endregion

            var subject = new UserController(userService.Object, tokenService.Object);
            var response = subject.ChangePassword(BuildChangePasswordViewModel(), "SOMETHING");
            Assert.AreEqual((new BadRequestResult()).GetType(), response.GetType());
        }

        [TestMethod]
        public void TestChangePasswordWithValidUser_ShouldReturnOk()
        {
            #region Mock
            var tokenService = new Mock<ITokenService<TokenViewModel, Token>>();
            tokenService.Setup(s => s.ValidateToken(It.IsAny<string>())).Returns(true);
            List<UserViewModel> mockedReturn = buildUserViewModelList();
            var userService = new Mock<IUserService<UserViewModel, User>>();
            userService.Setup(s => s.ChangePassword(It.IsAny<ChangePasswordViewModel>())).Returns(true);
            #endregion

            var subject = new UserController(userService.Object, tokenService.Object);
            var response = subject.ChangePassword(BuildChangePasswordViewModel(), "SOMETHING");
            Assert.AreEqual((new OkObjectResult(new { Message = "Password Changed" })).GetType(), response.GetType());
        }

        private List<UserViewModel> buildUserViewModelList()
        {
            return new List<UserViewModel>()
            {
                new UserViewModel { AccountId = 1, Email = "james.hetfield@algumprovedordeemail.com", Id = 1, Name = "James Hetfield", Password = "pass1", UserName = "user2" },
                new UserViewModel { AccountId = 1, Email = "lars.ulrich@algumprovedordeemail.com", Id = 2, Name = "Lars Ulrich", Password = "pass2", UserName = "user2" },
            };
        }

        private UserViewModel BuildUserViewModel()
        {
            return new UserViewModel()
            {
                AccountId = 123,
                Email = "user_view_model_email@algum_provedor.algum_pais",
                Id = 2342,
                Name = "New User",
                Password = "pegadinha_do_malandro",
                UserName = "new_user"
            };
        }

        private ChangePasswordViewModel BuildChangePasswordViewModel()
        {
            return new ChangePasswordViewModel()
            {
                Email = "metallica@someemailprovider.com",
                Id = 1,
                NewPassword = "metallica-hardwired",
                OldPassword = "metallica-death-magnetic"
            };
        }
    }
}
