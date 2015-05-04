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
    public class OfferServiceTests
    {
        #region Initialize
        private static void TestSetup(out FakeAuditRepository<Offer> repository, out OfferService service, out BasicOffer offer, out User user)
        {
            repository = new FakeAuditRepository<Offer>();
            service = new OfferService(
                repository,
                new NotificationService(new FakeRepository<Notification>()),
                new LikeService(new FakeRepository<Like>()),
                new FavoriteService(new FakeRepository<Favorite>()),
                new ShareService(new FakeRepository<Share>()),
                new CommentService(new FakeRepository<Comment>()),
                new RedemptionService(new FakeRepository<Redemption>()),
                new RewardService(new FakeRepository<Reward>()),
                new LoyaltyTransactionService(new FakeRepository<LoyaltyTransaction>()),
                new ElligibleTerminalService(new FakeRepository<ElligibleTerminal>()));

            offer = new BasicOffer { };
            user = new User { };
        }
        #endregion Initialize

        #region Add
        [TestMethod]
        public void CanAddBasicOffer()
        {
            // Arrange
            FakeAuditRepository<Offer> repository;
            OfferService service;
            BasicOffer offer;
            User user;
            TestSetup(out repository, out service, out offer, out user);

            // Act

            service.Add(offer, user);

            // Assert

            Assert.AreEqual(DateTime.Today, offer.CreationDate.Date);
            Assert.AreEqual(DateTime.Today, offer.Timestamp.Date);
            Assert.AreEqual(1, repository.All().Count());
        }

        [TestMethod]
        public void CantApproveOffersAsMerchant()
        {
            // Arrange
            FakeAuditRepository<Offer> repository;
            OfferService service;
            BasicOffer offer;
            User user;
            TestSetup(out repository, out service, out offer, out user);

            // Act
            service.Add(offer, user);
            
            // Assert

            Assert.AreEqual(DateTime.Today, offer.CreationDate.Date);
            Assert.AreEqual(offer.IsAdminApproved, false);
        }


        [TestMethod]
        public void CanAddLoyaltyOffer()
        {
            // Arrange
            FakeAuditRepository<Offer> repository;
            OfferService service;
            BasicOffer offer1;
            LoyaltyOffer offer = new LoyaltyOffer() { TargetTransactionTotal = null, TargetDollarTotal = 100};
            User user;
            TestSetup(out repository, out service, out offer1, out user);

            // Act
            service.Add(offer, user);

            // Assert
            Assert.AreEqual(DateTime.Today, offer.CreationDate.Date);
            Assert.AreEqual(DateTime.Today, offer.Timestamp.Date);
            Assert.AreEqual(1, repository.All().Count());
        }

        [TestMethod]
        public void CanAddTargetedOffer()
        {
            // Arrange
            FakeAuditRepository<Offer> repository;
            OfferService service;
            BasicOffer offer1;
            TargetedOffer offer = new TargetedOffer() { };
            User user;
            TestSetup(out repository, out service, out offer1, out user);


            // Act
            service.Add(offer, user);

            // Assert

            Assert.AreEqual(DateTime.Today, offer.CreationDate.Date);
            Assert.AreEqual(DateTime.Today, offer.Timestamp.Date);
            Assert.AreEqual(1, repository.All().Count());
        }

        [TestMethod]
        public void CanAddExpiredTargetedOffer()
        {
            // Arrange
            FakeAuditRepository<Offer> repository;
            OfferService service;
            BasicOffer offer1;
            TargetedOffer offer = new TargetedOffer() { 
                ExpirationDate = DateTime.Now.AddDays(-11),
                StartDate = DateTime.Now.AddDays(10),
                IsDeleted = false
            };
            User user;
            TestSetup(out repository, out service, out offer1, out user);


            // Act
            service.Add(offer, user);

            // Assert

            Assert.AreEqual(1, repository.All().Count());
        }

        #endregion

        #region Update

        [TestMethod]
        public void UpdateOfferUpdatesTimestamp()
        {
            // Arrange
            FakeAuditRepository<Offer> repository;
            OfferService service;
            BasicOffer offer;
            User user;
            TestSetup(out repository, out service, out offer, out user);

            // Act

            service.Update(offer, user);

            // Assert

            Assert.AreEqual(DateTime.Today, offer.Timestamp.Date);
        }

        [TestMethod]
        public void CantUpdateOfferToExpired()
        {
            // Arrange
            FakeAuditRepository<Offer> repository;
            OfferService service;
            BasicOffer offer = new BasicOffer()
            {
                ExpirationDate = DateTime.Now.AddDays(-10),
                StartDate = DateTime.Now.AddDays(10)
            };
            User user;
            TestSetup(out repository, out service, out offer, out user);

            // Act

            service.Update(offer, user);

            // Assert

            Assert.AreEqual(DateTime.Today, offer.Timestamp.Date);
        }
        #endregion

        #region Remove
        [TestMethod]
        public void RemoveOfferBasic()
        {
            // Arrange
            FakeAuditRepository<Offer> repository = new FakeAuditRepository<Offer>();
            OfferService service = new OfferService(
                repository,
                new NotificationService(new FakeRepository<Notification>()),
                new LikeService(new FakeRepository<Like>()),
                new FavoriteService(new FakeRepository<Favorite>()),
                new ShareService(new FakeRepository<Share>()),
                new CommentService(new FakeRepository<Comment>()),
                new RedemptionService(new FakeRepository<Redemption>()),
                new RewardService(new FakeRepository<Reward>()),
                new LoyaltyTransactionService(new FakeRepository<LoyaltyTransaction>()),
                new ElligibleTerminalService(new FakeRepository<ElligibleTerminal>()));

            User user = new User { };

            BasicOffer offer = new BasicOffer()
            {
                Likes = new List<Like>(){
                    new Like(){
                        IsDeleted = false
                    }
                },
                Favorites = new List<Favorite>(){
                    new Favorite(){
                        IsDeleted = false
                    }
                },
                Shares = new List<Share>(){
                    new Share(){
                        IsDeleted = false
                    }
                },
                Comments = new List<Comment>(){
                    new Comment(){
                        IsDeleted = false
                    }
                },
                Redemptions = new List<Redemption>(){
                    new Redemption(){
                        Offer = new BasicOffer(){}
                    }
                }
            };

            //Act
            service.Remove(offer, user);

            //Assert
            Assert.IsTrue(offer.Likes.FirstOrDefault().IsDeleted);
            Assert.IsTrue(offer.Favorites.FirstOrDefault().IsDeleted);
            Assert.IsTrue(offer.Shares.FirstOrDefault().IsDeleted);
            Assert.IsTrue(offer.Comments.FirstOrDefault().IsDeleted);
            Assert.IsTrue(offer.Redemptions.FirstOrDefault().IsDeleted);
            Assert.IsTrue(offer.IsDeleted);
        }

        [TestMethod]
        public void RemoveOfferLoyalty()
        {
            // Arrange
            FakeAuditRepository<Offer> repository = new FakeAuditRepository<Offer>();
            OfferService service = new OfferService(
                repository,
                new NotificationService(new FakeRepository<Notification>()),
                new LikeService(new FakeRepository<Like>()),
                new FavoriteService(new FakeRepository<Favorite>()),
                new ShareService(new FakeRepository<Share>()),
                new CommentService(new FakeRepository<Comment>()),
                new RedemptionService(new FakeRepository<Redemption>()),
                new RewardService(new FakeRepository<Reward>()),
                new LoyaltyTransactionService(new FakeRepository<LoyaltyTransaction>()),
                new ElligibleTerminalService(new FakeRepository<ElligibleTerminal>()));

            User user = new User { };
            LoyaltyOffer offer = new LoyaltyOffer()
            {
                Likes = new List<Like>(){
                    new Like(){
                        IsDeleted = false
                    }
                },
                Favorites = new List<Favorite>(){
                    new Favorite(){
                        IsDeleted = false
                    }
                },
                Shares = new List<Share>(){
                    new Share(){
                        IsDeleted = false
                    }
                },
                Comments = new List<Comment>(){
                    new Comment(){
                        IsDeleted = false
                    }
                },
                Rewards = new List<Reward>(){
                    new Reward(){
                        Offer = new LoyaltyOffer(){}
                    }
                }
            };

            //Act
            service.Remove(offer, user);

            //Assert
            Assert.IsTrue(offer.Likes.FirstOrDefault().IsDeleted);
            Assert.IsTrue(offer.Favorites.FirstOrDefault().IsDeleted);
            Assert.IsTrue(offer.Shares.FirstOrDefault().IsDeleted);
            Assert.IsTrue(offer.Comments.FirstOrDefault().IsDeleted);
            Assert.IsTrue(offer.Rewards.FirstOrDefault().IsDeleted);
            Assert.IsTrue(offer.IsDeleted);
        }

        [TestMethod]
        public void RemoveOfferTargeted()
        {
            // Arrange
            FakeAuditRepository<Offer> repository = new FakeAuditRepository<Offer>();
            OfferService service = new OfferService(
                repository,
                new NotificationService(new FakeRepository<Notification>()),
                new LikeService(new FakeRepository<Like>()),
                new FavoriteService(new FakeRepository<Favorite>()),
                new ShareService(new FakeRepository<Share>()),
                new CommentService(new FakeRepository<Comment>()),
                new RedemptionService(new FakeRepository<Redemption>()),
                new RewardService(new FakeRepository<Reward>()),
                new LoyaltyTransactionService(new FakeRepository<LoyaltyTransaction>()),
                new ElligibleTerminalService(new FakeRepository<ElligibleTerminal>()));

            TargetedOffer offer = new TargetedOffer { };
            User user = new User { };

            offer.Rewards = new List<Reward>(){
                new Reward(){
                    Offer = new LoyaltyOffer(){}
                }
            };

            //Act
            service.Remove(offer, user);

            //Assert
            Assert.IsTrue(offer.Rewards.FirstOrDefault().IsDeleted);
            Assert.IsTrue(offer.IsDeleted);
        }
        #endregion

        #region Redeem

        [TestMethod]
        public void RedeemBasicOffer()
        {
            // Arrange
            var redemptionRepository = new FakeRepository<Redemption>();
            FakeAuditRepository<Offer> repository = new FakeAuditRepository<Offer>();
            OfferService service = new OfferService(
                repository,
                new NotificationService(new FakeRepository<Notification>()),
                new LikeService(new FakeRepository<Like>()),
                new FavoriteService(new FakeRepository<Favorite>()),
                new ShareService(new FakeRepository<Share>()),
                new CommentService(new FakeRepository<Comment>()),
                new RedemptionService(redemptionRepository),
                new RewardService(new FakeRepository<Reward>()),
                new LoyaltyTransactionService(new FakeRepository<LoyaltyTransaction>()),
                new ElligibleTerminalService(new FakeRepository<ElligibleTerminal>()));

            BasicOffer offer = new BasicOffer { 
                ExpirationDate = DateTime.Now.AddDays(1),
                IsAdminApproved = true
            };
            Member member = new Member { };

            //Act
            offer = (BasicOffer)service.Redeem(offer, member);

            //Assert
            Assert.IsTrue(redemptionRepository.Any(x => x.Offer == offer));
        }

        [TestMethod]
        public void RedeemLoyaltyOffer()
        {
            // Arrange
            Member member = new Member
            {
                Profile = new Profile()
                {
                    Name = "Eddie",
                    Email = "egoynes@mapscu.com",
                    Zip = "97330",
                    MemberNumber = "1234567"
                },
                MemberId = 2,
                Rewards = new List<Reward>()
                {
                new Reward(){
                        EarnedDate = DateTime.Now,
                        OfferId = 1,
                        MemberId = 2
                     }
                }
            };
            var rewardRepository = new FakeRepository<Reward>();
            rewardRepository.Add( 
                new Reward(){
                    EarnedDate = DateTime.Now,
                    OfferId = 1,
                    Member = member,
                    MemberId = 2,
                    ExpirationDate = DateTime.Now.AddDays(10)
            });
            FakeAuditRepository<Offer> repository = new FakeAuditRepository<Offer>();
            OfferService service = new OfferService(
                repository,
                new NotificationService(new FakeRepository<Notification>()),
                new LikeService(new FakeRepository<Like>()),
                new FavoriteService(new FakeRepository<Favorite>()),
                new ShareService(new FakeRepository<Share>()),
                new CommentService(new FakeRepository<Comment>()),
                new RedemptionService(new FakeRepository<Redemption>()),
                new RewardService(rewardRepository),
                new LoyaltyTransactionService(new FakeRepository<LoyaltyTransaction>()),
                new ElligibleTerminalService(new FakeRepository<ElligibleTerminal>()));

            LoyaltyOffer offer = new LoyaltyOffer {
                OfferId = 1,
                ExpirationDate = DateTime.Now.AddDays(1),
                StartDate = DateTime.Now.AddDays(-1),
                IsAdminApproved = true
            };

            //Acts
            service.Redeem(offer, member);

            //Assert
            Assert.IsTrue(rewardRepository.Any(x => x.RedemptionDate != null));
        }

        [TestMethod]
        public void CantRedeemDeletedLoyaltyOffer()
        {
            // Arrange
            Member member = new Member
            {
                Profile = new Profile()
                {
                    Name = "Eddie",
                    Email = "egoynes@mapscu.com",
                    Zip = "97330",
                    MemberNumber = "1234567"
                },
                MemberId = 2,
                Rewards = new List<Reward>()
                {
                new Reward(){
                        EarnedDate = DateTime.Now,
                        OfferId = 1,
                        MemberId = 2
                     }
                }
            };
            var rewardRepository = new FakeRepository<Reward>();
            rewardRepository.Add(
                new Reward()
                {
                    EarnedDate = DateTime.Now,
                    OfferId = 1,
                    Member = member,
                    MemberId = 2,
                    ExpirationDate = DateTime.Now.AddDays(10)
                });
            FakeAuditRepository<Offer> repository = new FakeAuditRepository<Offer>();
            OfferService service = new OfferService(
                repository,
                new NotificationService(new FakeRepository<Notification>()),
                new LikeService(new FakeRepository<Like>()),
                new FavoriteService(new FakeRepository<Favorite>()),
                new ShareService(new FakeRepository<Share>()),
                new CommentService(new FakeRepository<Comment>()),
                new RedemptionService(new FakeRepository<Redemption>()),
                new RewardService(rewardRepository),
                new LoyaltyTransactionService(new FakeRepository<LoyaltyTransaction>()),
                new ElligibleTerminalService(new FakeRepository<ElligibleTerminal>()));

            LoyaltyOffer offer = new LoyaltyOffer
            {
                OfferId = 1,
                ExpirationDate = DateTime.Now.AddDays(1),
                StartDate = DateTime.Now.AddDays(-1),
                IsDeleted = false,
                IsAdminApproved = true,
                CreationDate = DateTime.Now.AddDays(10)
            };

            //Acts
            service.Redeem(offer, member);

            //Assert
            Assert.IsTrue(rewardRepository.Any(x => x.RedemptionDate != null));
        }
        #endregion

        #region Find By Institution

        [TestMethod]
        public void CanFindByInstitution()
        {
            // Arrange
            Member member = new Member { InstitutionId = 1 };
            var offerList = new List<Offer>(){
                new BasicOffer(){
                    InstitutionId = 1,
                    ExpirationDate = DateTime.Now.AddDays(10),
                    StartDate = DateTime.Now.AddDays(-10),
                    IsAdminApproved = true,
                    OfferId = 1,
                    IsHidden = false,
                    Merchant = new Merchant(){
                        Locations = new List<MerchantLocation>(){
                            new MerchantLocation(){
                                IsDeleted = false
                            }
                        }
                    }
                }
            };
            //Add all of the offers to my offer repository that i've created. 
            FakeAuditRepository<Offer> repository = new FakeAuditRepository<Offer>(offerList);

            OfferService service = new OfferService(
                repository,
                new NotificationService(new FakeRepository<Notification>()),
                new LikeService(new FakeRepository<Like>()),
                new FavoriteService(new FakeRepository<Favorite>()),
                new ShareService(new FakeRepository<Share>()),
                new CommentService(new FakeRepository<Comment>()),
                new RedemptionService(new FakeRepository<Redemption>()),
                new RewardService(new FakeRepository<Reward>()),
                new LoyaltyTransactionService(new FakeRepository<LoyaltyTransaction>()),
                new ElligibleTerminalService(new FakeRepository<ElligibleTerminal>()));

            BasicOffer offer = new BasicOffer {};

            //Act
            var returnedOffer = (Offer)service.FindByInstitution(1, 1, false);

            //Assert
            Assert.AreEqual(1, returnedOffer.InstitutionId);
        }




        #endregion

        #region Get Avalable Public Offers

        [TestMethod]
        public void CanGetAvalablePublicOffers()
        {
            // Arrange
            Member member = new Member {InstitutionId = 1};
            var offerList = new List<Offer>(){
                new BasicOffer(){
                    InstitutionId = 1,
                    ExpirationDate = DateTime.Now.AddDays(10),
                    StartDate = DateTime.Now.AddDays(-10),
                    IsAdminApproved = true,
                    IsHidden = false,
                    Merchant = new Merchant(){
                        Locations = new List<MerchantLocation>(){
                            new MerchantLocation(){
                                IsDeleted = false
                            }
                        }
                    }
                },
                 new BasicOffer(){
                    InstitutionId = 1,
                    ExpirationDate = DateTime.Now.AddDays(-5),
                    StartDate = DateTime.Now.AddDays(-10),
                    IsAdminApproved = true,
                    IsHidden = false,
                    Merchant = new Merchant(){
                        Locations = new List<MerchantLocation>(){
                            new MerchantLocation(){
                                IsDeleted = false
                            }
                        }
                    }
                },
                 new LoyaltyOffer(){
                    InstitutionId = 1,
                    ExpirationDate = DateTime.Now.AddDays(20),
                    StartDate = DateTime.Now.AddDays(10),
                    IsAdminApproved = true,
                    IsHidden = false,
                    Merchant = new Merchant(){
                        Locations = new List<MerchantLocation>(){
                            new MerchantLocation(){
                                IsDeleted = false
                            }
                        }
                    }
                },
                 new LoyaltyOffer(){
                    InstitutionId = 1,
                    ExpirationDate = DateTime.Now.AddDays(10),
                    StartDate = DateTime.Now.AddDays(-10),
                    IsAdminApproved = false,
                    IsHidden = false,
                    Merchant = new Merchant(){
                        Locations = new List<MerchantLocation>(){
                            new MerchantLocation(){
                                IsDeleted = false
                            }
                        }
                    }
                }
            };
            //Add all of the offers to my offer repository that i've created. 
            FakeAuditRepository<Offer> repository = new FakeAuditRepository<Offer>(offerList);

            OfferService service = new OfferService(
                repository,
                new NotificationService(new FakeRepository<Notification>()),
                new LikeService(new FakeRepository<Like>()),
                new FavoriteService(new FakeRepository<Favorite>()),
                new ShareService(new FakeRepository<Share>()),
                new CommentService(new FakeRepository<Comment>()),
                new RedemptionService(new FakeRepository<Redemption>()),
                new RewardService(new FakeRepository<Reward>()),
                new LoyaltyTransactionService(new FakeRepository<LoyaltyTransaction>()),
                new ElligibleTerminalService(new FakeRepository<ElligibleTerminal>()));

            BasicOffer offer = new BasicOffer { };

            //Act
            IEnumerable<Offer> returnedRepository = service.GetAvailablePublicOffers(1);

            //Assert
            Assert.AreEqual(1 ,returnedRepository.First().InstitutionId);
            Assert.IsTrue(returnedRepository.Count() == 1);
        }


        #endregion

        #region Get Avalable Offers

        [TestMethod]
        public void CanGetAvalableOffers()
        {
            // Arrange

            var member = new Member 
            { 
                MemberId = 1,
                InstitutionId = 1 
            };

            var repository = new FakeAuditRepository<Offer>(new List<Offer> 
            { 
                new BasicOffer { // Avalable basic offer
                    OfferId = 1,
                    InstitutionId = 1,
                    ExpirationDate = DateTime.Now.AddYears(1),
                    StartDate = DateTime.Now.AddYears(-1),
                    Timestamp = DateTime.Now.AddMinutes(-1),
                    IsAdminApproved = true,
                    Merchant = new Merchant {
                        Locations = new List<MerchantLocation> {
                            new MerchantLocation {
                                IsDeleted = false
                            }
                        }
                    }
                },

                new LoyaltyOffer { // Avalable loyalty offer
                    OfferId = 2,
                    InstitutionId = 1,
                    ExpirationDate = DateTime.Now.AddYears(1),
                    StartDate = DateTime.Now.AddYears(-1),
                    Timestamp = DateTime.Now.AddMinutes(-1),
                    IsAdminApproved = true,
                    Merchant = new Merchant {
                        Locations = new List<MerchantLocation> {
                            new MerchantLocation {
                                IsDeleted = false
                            }
                        }
                    }
                },

                new TargetedOffer { // Avalable targeted offer
                    OfferId = 3,
                    InstitutionId = 1,
                    ExpirationDate = DateTime.Now.AddYears(1),
                    StartDate = DateTime.Now.AddYears(-1),
                    Timestamp = DateTime.Now.AddMinutes(-1),
                    IsAdminApproved = true,
                    Merchant = new Merchant {
                        Locations = new List<MerchantLocation> {
                            new MerchantLocation {
                                IsDeleted = false
                            }
                        }
                    },
                    Rewards = new List<Reward> {
                        new Reward {
                            MemberId = 1,
                            RedemptionDate = null,
                            ExpirationDate = DateTime.Now.AddYears(1)
                        }
                    }
                },

                new BasicOffer {
                    OfferId = 4,
                    InstitutionId = 1,
                    ExpirationDate = DateTime.Now.AddYears(1),
                    StartDate = DateTime.Now.AddYears(-1),
                    Timestamp = DateTime.Now.AddMinutes(-1),
                    IsAdminApproved = false, // Not approved
                    Merchant = new Merchant {
                        Locations = new List<MerchantLocation> {
                            new MerchantLocation {
                                IsDeleted = false
                            }
                        }
                    }
                },

                new BasicOffer {
                    OfferId = 5,
                    InstitutionId = 2, // Different institution
                    ExpirationDate = DateTime.Now.AddYears(1),
                    StartDate = DateTime.Now.AddYears(-1),
                    Timestamp = DateTime.Now.AddMinutes(-1),
                    IsAdminApproved = true, 
                    Merchant = new Merchant {
                        Locations = new List<MerchantLocation> {
                            new MerchantLocation {
                                IsDeleted = false
                            }
                        }
                    }
                },

                new BasicOffer {
                    OfferId = 6,
                    InstitutionId = 1,
                    ExpirationDate = DateTime.Now.AddYears(1),
                    StartDate = DateTime.Now.AddYears(-1),
                    Timestamp = DateTime.Now.AddMinutes(-1),
                    IsAdminApproved = true, 
                    Merchant = new Merchant {
                        Locations = new List<MerchantLocation> { // No locations
                            new MerchantLocation {
                                IsDeleted = true
                            }
                        }
                    }
                },

                new LoyaltyOffer {
                    OfferId = 7,
                    InstitutionId = 1,
                    ExpirationDate = DateTime.Now.AddYears(-1), // Expired
                    StartDate = DateTime.Now.AddYears(-1),
                    Timestamp = DateTime.Now.AddMinutes(-1),
                    IsAdminApproved = true, 
                    Merchant = new Merchant {
                        Locations = new List<MerchantLocation> {
                            new MerchantLocation {
                                IsDeleted = false
                            }
                        }
                    }
                },

                new LoyaltyOffer {
                    OfferId = 8,
                    InstitutionId = 1,
                    ExpirationDate = DateTime.Now.AddYears(1),
                    StartDate = DateTime.Now.AddYears(1), // Hasn't started yet
                    Timestamp = DateTime.Now.AddMinutes(-1),
                    IsAdminApproved = true, 
                    Merchant = new Merchant {
                        Locations = new List<MerchantLocation> {
                            new MerchantLocation {
                                IsDeleted = false
                            }
                        }
                    }
                },

                new LoyaltyOffer {
                    OfferId = 9,
                    InstitutionId = 1,
                    ExpirationDate = DateTime.Now.AddYears(1),
                    StartDate = DateTime.Now.AddYears(-1),
                    Timestamp = DateTime.Now.AddMinutes(-1),
                    IsAdminApproved = true, 
                    IsHidden = true, // Hidden
                    Merchant = new Merchant {
                        Locations = new List<MerchantLocation> {
                            new MerchantLocation {
                                IsDeleted = false
                            }
                        }
                    }
                },

                new LoyaltyOffer {
                    OfferId = 10,
                    InstitutionId = 1,
                    ExpirationDate = DateTime.Now.AddYears(1),
                    StartDate = DateTime.Now.AddYears(-1),
                    Timestamp = DateTime.Now.AddMinutes(-1),
                    IsAdminApproved = true, 
                    IsDeleted = true, // Deleted
                    Merchant = new Merchant {
                        Locations = new List<MerchantLocation> {
                            new MerchantLocation {
                                IsDeleted = false
                            }
                        }
                    }
                },

                new TargetedOffer {
                    OfferId = 11,
                    InstitutionId = 1,
                    ExpirationDate = DateTime.Now.AddYears(1),
                    StartDate = DateTime.Now.AddYears(-1),
                    Timestamp = DateTime.Now.AddMinutes(-1),
                    IsAdminApproved = true, 
                    Merchant = new Merchant {
                        Locations = new List<MerchantLocation> {
                            new MerchantLocation {
                                IsDeleted = false
                            }
                        }
                    },
                    Rewards = new List<Reward> { // No available rewards
                        new Reward {
                            MemberId = 2, // Different member
                            RedemptionDate = null,
                            ExpirationDate = DateTime.Now.AddYears(1)
                        },
                        new Reward {
                            MemberId = 1,
                            RedemptionDate = DateTime.Now, // Redeemed
                            ExpirationDate = DateTime.Now.AddYears(1)
                        },
                        new Reward {
                            MemberId = 1,
                            RedemptionDate = null,
                            ExpirationDate = DateTime.Now.AddYears(-1) // Expired
                        },
                        new Reward {
                            MemberId = 1,
                            RedemptionDate = null,
                            ExpirationDate = DateTime.Now.AddYears(1),
                            IsDeleted = true // Deleted
                        }
                    }
                }
            });

            var service = new OfferService(
                repository,
                new NotificationService(new FakeRepository<Notification>()),
                new LikeService(new FakeRepository<Like>()),
                new FavoriteService(new FakeRepository<Favorite>()),
                new ShareService(new FakeRepository<Share>()),
                new CommentService(new FakeRepository<Comment>()),
                new RedemptionService(new FakeRepository<Redemption>()),
                new RewardService(new FakeRepository<Reward>()),
                new LoyaltyTransactionService(new FakeRepository<LoyaltyTransaction>()),
                new ElligibleTerminalService(new FakeRepository<ElligibleTerminal>()));

            // Act
            var results = service.GetAvailableOffers(member);

            // Assert
            Assert.AreEqual(3, results.Count());
            Assert.IsTrue(results.Any(x => x.OfferId == 1));
            Assert.IsTrue(results.Any(x => x.OfferId == 2));
            Assert.IsTrue(results.Any(x => x.OfferId == 3));
        }

        [TestMethod]
        public void OrderGetAvalableOffers()
        {
            // Arrange

            var member = new Member
            {
                MemberId = 1,
                InstitutionId = 1
            };

            var repository = new FakeAuditRepository<Offer>(new List<Offer> 
            { 
                new TargetedOffer { // Targeted offer
                    OfferId = 2,
                    InstitutionId = 1,
                    ExpirationDate = DateTime.Now.AddYears(1),
                    StartDate = DateTime.Now.AddYears(-1),
                    CreationDate = DateTime.Now.AddMinutes(-2),
                    IsAdminApproved = true,
                    Merchant = new Merchant {
                        Locations = new List<MerchantLocation> {
                            new MerchantLocation {
                                IsDeleted = false
                            }
                        }
                    },
                    Rewards = new List<Reward> {
                        new Reward {
                            MemberId = 1,
                            RedemptionDate = null,
                            ExpirationDate = DateTime.Now.AddYears(1)
                        }
                    }
                },

                new BasicOffer {
                    OfferId = 3,
                    InstitutionId = 1,
                    ExpirationDate = DateTime.Now.AddYears(1),
                    StartDate = DateTime.Now.AddYears(-1),
                    CreationDate = DateTime.Now.AddMinutes(-3),
                    IsAdminApproved = true,
                    Merchant = new Merchant {
                        Locations = new List<MerchantLocation> {
                            new MerchantLocation {
                                IsDeleted = false
                            }
                        }
                    }
                },

                new BasicOffer {
                    OfferId = 4,
                    InstitutionId = 1,
                    ExpirationDate = DateTime.Now.AddYears(1),
                    StartDate = DateTime.Now.AddYears(-1),
                    CreationDate = DateTime.Now.AddMinutes(-4),
                    IsAdminApproved = true,
                    Merchant = new Merchant {
                        Locations = new List<MerchantLocation> {
                            new MerchantLocation {
                                IsDeleted = false
                            }
                        }
                    }
                },

                new BasicOffer {
                    OfferId = 1,
                    InstitutionId = 1,
                    ExpirationDate = DateTime.Now.AddYears(1),
                    StartDate = DateTime.Now.AddYears(-1),
                    CreationDate = DateTime.Now.AddMinutes(-1),
                    IsAdminApproved = true,
                    IsFeatured = true, // Feautred
                    Merchant = new Merchant {
                        Locations = new List<MerchantLocation> {
                            new MerchantLocation {
                                IsDeleted = false
                            }
                        }
                    }
                },
            });

            var service = new OfferService(
                repository,
                new NotificationService(new FakeRepository<Notification>()),
                new LikeService(new FakeRepository<Like>()),
                new FavoriteService(new FakeRepository<Favorite>()),
                new ShareService(new FakeRepository<Share>()),
                new CommentService(new FakeRepository<Comment>()),
                new RedemptionService(new FakeRepository<Redemption>()),
                new RewardService(new FakeRepository<Reward>()),
                new LoyaltyTransactionService(new FakeRepository<LoyaltyTransaction>()),
                new ElligibleTerminalService(new FakeRepository<ElligibleTerminal>()));

            // Act
            var results = service.GetAvailableOffers(member);

            // Assert - Featured offers first
            Assert.AreEqual(1, results.ElementAt(0).OfferId);

            // Assert - Targeted offer second
            Assert.AreEqual(2, results.ElementAt(1).OfferId);

            // Offers CreationDate descending
            Assert.AreEqual(3, results.ElementAt(2).OfferId);
            Assert.AreEqual(4, results.ElementAt(3).OfferId);
        }

        #endregion

        #region Get by institution

        [TestMethod]
        public void CanGetOffersByInstitution()
        {
            // Arrange
            
            var institutionId = 1;

            var offers = new List<Offer>
            {
                new BasicOffer { InstitutionId = 1 },
                new BasicOffer { InstitutionId = 2 } // Different InstitutionId
            };
            
            var service = new OfferService(
                new FakeAuditRepository<Offer>(offers),
                new NotificationService(new FakeRepository<Notification>()),
                new LikeService(new FakeRepository<Like>()),
                new FavoriteService(new FakeRepository<Favorite>()),
                new ShareService(new FakeRepository<Share>()),
                new CommentService(new FakeRepository<Comment>()),
                new RedemptionService(new FakeRepository<Redemption>()),
                new RewardService(new FakeRepository<Reward>()),
                new LoyaltyTransactionService(new FakeRepository<LoyaltyTransaction>()),
                new ElligibleTerminalService(new FakeRepository<ElligibleTerminal>()));

            // Act

            var results = service.GetByInstitution(institutionId);

            // Assert

            Assert.AreEqual(1, results.Count());
            Assert.AreEqual(0, results.Where(x => x.InstitutionId != institutionId).Count());
        }

        #endregion
    }
}