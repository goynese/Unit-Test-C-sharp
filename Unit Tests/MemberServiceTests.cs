using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BuyLocal.Core.Domain;
using System.Collections.Generic;
using BuyLocal.Core.Service;
using System.Linq;
using BuyLocal.Core.Data;

namespace BuyLocal.Service.Tests
{
    [TestClass]
    public class MemberServiceTests
    {
        #region Initialize

        private static List<Member> GetMemberList()
        {
            var memberList = new List<Member>(){
                new Member(){
                    Username = "user1",
                    MemberId = 5,
                    InstitutionId = 1,
                    Profile = new Profile(){
                        Email = "fakeemail@test.com",
                        ImageUrl = "\fakeimage.png",
                        Name = "Eddie",
                        Zip = "97301",
                        MemberNumber = "1234567"
                    }
                },
                new Member(){
                    Username = "user2",
                    MemberId = 6,
                    InstitutionId = 1,
                }
            };
            return memberList;
        }

        #endregion 

        #region CanFindByUsername

        [TestMethod]
        public void CanFindByUsername()
        {
            // Arrange
            var memberList = GetMemberList();

            FakeRepository<Member> repository = new FakeRepository<Member>(memberList);
            
            MemberService service = new MemberService(repository);
        
            //Act
            var returnedMember = service.FindByUsername("user1", 1);

            //Assert
            Assert.IsTrue(returnedMember.Username == "user1");
        }
        #endregion

        #region CanAddMember

        [TestMethod]
        public void CanAddMember()
        {
            // Arrange
            List<Member> memberList = GetMemberList();

            Member member = new Member(){
                    Username = "specialMember",
                    MemberId = 6,
                    InstitutionId = 1,           
                    AuthType = AuthType.OnlineBanking,
                    Profile = new Profile(){
                        Email = "fakeemail@test.com",
                        ImageUrl = "\fakeimage.png",
                        Name = "Eddie",
                        Zip = "97301",
                        MemberNumber = "1234568"
                    }
            };
            FakeRepository<Member> repository = new FakeRepository<Member>(memberList);

            MemberService service = new MemberService(repository);

            //Act
            Member returnedMember = service.Add(member);
                
            //Assert
            Assert.AreEqual(returnedMember, member);
        }
        #endregion

        #region CanUpdateMember

        [TestMethod]
        public void CanUpdateMember()
        {
            // Arrange
            List<Member> memberList = GetMemberList();

            Member member = new Member()
            {
                Username = "user3",
                MemberId = 6,
                InstitutionId = 1,
                Profile = new Profile()
                {
                    Email = "fakeemail@test.com",
                    ImageUrl = "\fakeimage.png",
                    Name = "Eddie",
                    Zip = "97301",
                    MemberNumber = "1234568"
                }
            };

            FakeRepository<Member> repository = new FakeRepository<Member>(memberList);

            MemberService service = new MemberService(repository);

            //Act
            Member returnedMember = service.Update(member);

            //Assert
            Assert.AreEqual(returnedMember, member);
        }
        #endregion

        #region CanFindOrCreateMember

        [TestMethod]
        public void CanFindMember()
        {
            // Arrange
            List<Member> memberList = GetMemberList();

            Member anonymous = new Member(){
                AuthType = AuthType.Anonymous,
                Username = "anonymous",
                Profile = new Profile(){
                    Email = "eddiegoynes@yahoo.com",
                    Name = "Eddie",
                    ImageUrl = "fakeimage.png",
                    Zip = "97301"
                },
                Shares = new List<Share>(){
                    new Share(){
                        OfferId = 10,
                        MemberId = 5
                    }
                },
                Favorites = new List<Favorite>(){
                    new Favorite(){
                        OfferId = 10,
                    }
                },
                Likes = new List<Like>(){
                    new Like(){
                        MemberId = 10,
                        OfferId = 10
                    }
                }
            };

            FakeRepository<Member> repository = new FakeRepository<Member>(memberList);

            MemberService service = new MemberService(repository);

            //Act
            Member returnedMember = service.FindOrCreate("anonymous", 1, AuthType.OnlineBanking, anonymous);

            //Assert
            Assert.AreEqual(returnedMember, anonymous);
        }

        [TestMethod]
        public void CreateAnonymousMemberFromAnonymous()
        {
            // Arrange
            Member anonymous = new Member()
                    {
                        AuthType = AuthType.Anonymous,
                    };

            List<Member> memberList = new List<Member>(){
                anonymous
            };

            anonymous.Username = new Guid().ToString();

            FakeRepository<Member> repository = new FakeRepository<Member>(memberList);

            MemberService service = new MemberService(repository);
            //Act
            Member returnedMember = service.FindOrCreate(new Guid().ToString(), 1, AuthType.Anonymous, null);

            //Assert
            Assert.IsTrue(returnedMember.Username == new Guid().ToString());
        }

        [TestMethod]
        public void CanFindAndUpdateAnonymous()
        {
            // Arrange
            Member anonymous = new Member()
            {
                AuthType = AuthType.Anonymous,
                InstitutionId = 1,
                Username = new Guid().ToString(),
                Profile = new Profile()
                {
                    Email = "eddiegoynes@yahoo.com",
                    ImageUrl = "urlfake.com",
                    Name = "Eddie",
                    Zip = "546465456456",
                    MemberNumber = "87123957398573"
                }
            };

            List<Member> memberList = new List<Member>(){
                anonymous
            };

            anonymous.Username = new Guid().ToString();

            FakeRepository<Member> repository = new FakeRepository<Member>(memberList);

            MemberService service = new MemberService(repository);
            //Act
            Member returnedMember = service.FindOrCreate(new Guid().ToString(), 1, AuthType.Anonymous, anonymous);

            //Assert
            Assert.IsTrue(returnedMember.Username == new Guid().ToString());
        }



        [TestMethod]
        public void FindMemberOnlineBanking()
        {
            // Arrange
            Member anonymous = new Member()
            {
                AuthType = AuthType.OnlineBanking,
                InstitutionId = 1,
                Username = "user12",
                Profile = new Profile()
                {
                    Email = "eddiegoynes@yahoo.com",
                    ImageUrl = "urlfake.com",
                    Name = "Eddie",
                    Zip = "546465456456",
                    MemberNumber = "87123957398573"
                }
            };

            List<Member> memberList = GetMemberList();
            memberList.Add(anonymous);

            FakeRepository<Member> repository = new FakeRepository<Member>(memberList);
            MemberService service = new MemberService(repository);


            //Act
            Member returnedMember = service.FindOrCreate("user12", 1, AuthType.OnlineBanking, null);

            //Assert
            Assert.AreEqual(returnedMember, anonymous);
        }


        [TestMethod]
        public void ConvertAnonymousToOnineMember()
        {
            //Arrange
            Member anonymous = new Member()
            {
                AuthType = AuthType.Anonymous,
                InstitutionId = 1,
                Username = "user12",
                Profile = new Profile()
                {
                    Email = "eddiegoynes@yahoo.com",
                    ImageUrl = "urlfake.com",
                    Name = "Eddie",
                    Zip = "546465456456",
                    MemberNumber = "87123957398573"
                }
            };

            List<Member> memberList = GetMemberList();
            memberList.Add(anonymous);

            FakeRepository<Member> repository = new FakeRepository<Member>(memberList);
            MemberService service = new MemberService(repository);


            //Act
            Member returnedMember = service.FindOrCreate("user12", 1, AuthType.OnlineBanking, anonymous);

            //Assert
            Assert.AreEqual(returnedMember, anonymous);


        }

        //Create Anonymous member
        [TestMethod]
        public void CreateAnonymousMember()
        {
            //Arrange
            Member testMember = new Member()
            {
                AuthType = AuthType.Anonymous,
                InstitutionId = 1,
                Username = new Guid().ToString()
            };

            List<Member> memberList = GetMemberList();

            FakeRepository<Member> repository = new FakeRepository<Member>(memberList);
            MemberService service = new MemberService(repository);

            //Act
            Member returnedMember = service.FindOrCreate(new Guid().ToString(), 1, AuthType.Anonymous, null);

            //Assert
            Assert.AreEqual(returnedMember.InstitutionId, testMember.InstitutionId);
            Assert.AreEqual(returnedMember.AuthType, testMember.AuthType);
            Assert.AreEqual(returnedMember.Username, testMember.Username);
        }

        //Convert anonymous to regular member.
        [TestMethod]
        public void ConvertAnonymousToOnlineBanking()
        {
            //Arrange
            Member testMember = new Member()
            {
                AuthType = AuthType.Anonymous,
                InstitutionId = 1,
                Username = "fake",
                Profile = new Profile()
                {
                    Email = "test@email.com",
                    ImageUrl ="fakeUrl",
                    Name ="eddie",
                    Zip = "7413424234234232",
                    MemberNumber = "42394239823984"
                }
            };

            List<Member> memberList = GetMemberList();

            FakeRepository<Member> repository = new FakeRepository<Member>(memberList);
            MemberService service = new MemberService(repository);

            //Act
            Member returnedMember = service.FindOrCreate("testUser", 1, AuthType.OnlineBanking, testMember);

            //Assert
            Assert.AreEqual(returnedMember.InstitutionId, testMember.InstitutionId);
            Assert.AreEqual(returnedMember.AuthType, testMember.AuthType);
            Assert.AreEqual(returnedMember.Username, testMember.Username);
        }
       
        #endregion
    }
}
