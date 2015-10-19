## Overview

Firstly I follow the arrange act and assert methodology in these unit tests.
Secondly this unit testing code was made possible by creating a service layer that used dependency injection. 
Allowing the service layer to be agnostic to using a database interface or a fakerepository interface.
Dependency injection allows me to create a fake repository with test members, and then pass that into the corresponding 
services contructor. 

## Execution

Finally I'll explain my reasoning behind this sample unit test. One thing I want to note is that unit tests can always be more 
exhaustive, check more cases, look for more corner cases, but the fact is we don’t have infinite time, and things 
like code coverage should be taken into account.

## Sample Test Case

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
