using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BuyLocal.Web.Controllers;
using BuyLocal.Service;
using BuyLocal.Core.Domain;
using BuyLocal.Web.Models;
using System.Web.Mvc;
using System.Collections.Generic;
using Moq;
using System.Web;
using System.IO;
using System.Web.SessionState;
using System.Reflection;
using BuyLocal.Core.Data;

namespace BuyLocal.Web.Test
{
    [TestClass]
    public class AccountControllerTest
    {
        const string Username = "goynese";
        const string Token = "b2ef97b0-e1bc-4dc8-b293-e1dfd69d9f68";
        const string State = "http://localhost";
        const int InstitutionId = 1;
        string AnonymousUsername = Guid.NewGuid().ToString();

        #region Login

        [TestMethod]
        public void CanLogin()
        {
            //TODO: Mock controller request. 
            //Arrange 
            var controller = new AccountController(new MemberService(new FakeRepository<Member>()), new InstitutionService(new FakeRepository<Institution>()));
            
            //Act
            var result = controller.Login("/test");

            //Assert
            string BaseUrl = @"https://oauth.mapscu.com/";

            Assert.AreEqual(result, BaseUrl + "?client_id=FCD73EF5-9698-49B8-A458-015C1F8CECDC&response_type=token&state=/test&redirect_uri=" + "");
            
        }

        #endregion
        //Offers, Catagories, 

        [TestMethod]
        public void CanLogout()
        {
            //Arrange
            var institutionService = new InstitutionService(new FakeRepository<Institution>());

            var memberService = new MemberService(new FakeRepository<Member>());

            var controller = new AccountController(memberService, institutionService);

            HttpContext.Current = FakeHttpContext(Username, "Token" , true);
            
            //Act
            var result = (RedirectToRouteResult) controller.Logout();

            //Assert
            Assert.AreEqual("Index", result.RouteValues["action"]);
            Assert.AreEqual(null, HttpContext.Current.Session["Username"]);
            Assert.AreEqual(null, HttpContext.Current.Session["Token"]);
            Assert.AreEqual(false, (bool) HttpContext.Current.Session["IsRegistered"]);
        }


        #region CreateProfile

        [TestMethod]
        public void RedirectWorksCreateProfile()
        {
            //Arrange
            var controller = new AccountController(new MemberService(new FakeRepository<Member>()), new InstitutionService(new FakeRepository<Institution>()));

            //Act
            var result = (RedirectToRouteResult) controller.CreateProfile(null, "token", "state");

            //Assert
            Assert.AreEqual("Index", result.RouteValues["action"]);

        }

        [TestMethod]
        public void RedirectCreateProfile()
        {
            //Arrange
            var institutionService = new InstitutionService(new FakeRepository<Institution>());

            var memberService = new MemberService(new FakeRepository<Member>(new List<Member>
            {
                new Member {
                    Username = AnonymousUsername,
                    AuthType = AuthType.Anonymous,
                    InstitutionId = InstitutionId,
                    MemberId = 1,
                    MemberNumber = null,
                    Profile = new Profile{}
                }
            }));

            var controller = new AccountController(memberService, institutionService);

            HttpContext.Current = FakeHttpContext(AnonymousUsername, null, false);

            var testModel = new CreateProfile()
            {
                State = State,
                Username = Username,
                Token = Token
            };

            //Act
            var result = (RedirectToRouteResult)controller.CreateProfile(AnonymousUsername, Token, State);

            //Assert
            Assert.AreEqual("Index", result.RouteValues["action"]);
        }

        #endregion

        #region UpdateCreatedProfile
        [TestMethod]
        public void CanUpdateCreatedProfile()
        {
            //Arrage
            var institutionService = new InstitutionService(new FakeRepository<Institution>());

            var memberService = new MemberService(new FakeRepository<Member>(new List<Member>
            {
                new Member {
                    Username = AnonymousUsername,
                    AuthType = AuthType.Anonymous,
                    InstitutionId = InstitutionId,
                    MemberId = 1,
                    Profile = new Profile{}
                }, 
                new Member {
                    Username = Username,
                    AuthType = AuthType.OnlineBanking,
                    InstitutionId = InstitutionId,
                    MemberId = 2,
                    Profile = new Profile{}
                }
            }));

            var controller = new AccountController(memberService, institutionService);

            HttpContext.Current = FakeHttpContext(AnonymousUsername, null, false);

            //Act
            var result = (RedirectToRouteResult) controller.UpdateCreatedProfile(new CreateProfile(){
                Username = Username,
                Token = "b2ef97b0-e1bc-4dc8-b293-e1dfd69d9f68",
                State = "http://localhost",
                Name = "Eddie",
                Zip = "97302",
                Email = "egoynes@mapscu.com",
                MemberNumber = "1234567"
            });

            //Assert
            Assert.AreEqual("http://localhost", result.RouteValues["local"]);
        }

        #endregion 

        #region Manage

        [TestMethod]
        public void CanManageProfile()
        {
            //Arrange

            var institutionService = new InstitutionService(new FakeRepository<Institution>());

            var memberService = new MemberService(new FakeRepository<Member>(new List<Member>
            {
                new Member {
                    Username = Username,
                    AuthType = AuthType.OnlineBanking,
                    InstitutionId = InstitutionId,
                    MemberId = 1,
                    Profile = new Profile 
                    {
                        Email = "egoynes@mapscu.com",
                        Name = "Eddie",
                        Zip = "97302",
                        MemberNumber = "123456"
                    }
                }
            }));

            var controller = new AccountController(memberService, institutionService);

            // Username and token were pulled from sqlvm01.mapsoauth
            HttpContext.Current = FakeHttpContext("goynese", "b2ef97b0-e1bc-4dc8-b293-e1dfd69d9f68", true);

            //Act

            var result = (RedirectToRouteResult) controller.Manage(new AccountModel()
            {
                ProfileEmail = "carl@gmail.com",
                ProfileName = "MyName",
                ProfileZip = "97302"
            });

            //Assert

            var member = memberService.FindByUsername(Username, InstitutionId);

            Assert.AreEqual("Manage", result.RouteValues["action"]);
            Assert.AreEqual("carl@gmail.com", member.Profile.Email);
            Assert.AreEqual("MyName", member.Profile.Name);
            Assert.AreEqual("97302", member.Profile.Zip);
        }

        #endregion Manage

        public static HttpContext FakeHttpContext(string username, string token, bool isRegistered, string url = "http://localhost")
        {
            var uri = new Uri(url);

            var httpContext = new HttpContext(
                new HttpRequest(string.Empty, uri.ToString(), uri.Query.TrimStart('?')),
                new HttpResponse(new StringWriter()));

            var sessionContainer = new HttpSessionStateContainer(
                "id",
                new SessionStateItemCollection(),
                new HttpStaticObjectsCollection(),
                10,
                true,
                HttpCookieMode.AutoDetect,
                SessionStateMode.InProc,
                false);

            httpContext.Items["AspSession"] = typeof(HttpSessionState)
                .GetConstructor(
                    BindingFlags.NonPublic | BindingFlags.Instance,
                    null,
                    CallingConventions.Standard,
                    new[] { typeof(HttpSessionStateContainer) },
                    null)
                .Invoke(new object[] { sessionContainer });

            httpContext.Session["Username"] = username;
            httpContext.Session["IsRegistered"] = isRegistered;
            httpContext.Session["Token"] = token;

            return httpContext;
        }
    }
}
