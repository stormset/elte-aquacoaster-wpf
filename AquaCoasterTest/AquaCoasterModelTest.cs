using System;
using System.Collections.Generic;
using AquaCoaster.Model;
using AquaCoaster.Model.Entities;
using AquaCoaster.Persistence;
using AquaCoaster.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AquaCoasterTest
{
    [TestClass]
    public class AquaCoasterModelTest
    {
        private Mock<GameData> _mock;
        private GameModel _model;

        [TestInitialize]
        public void Initialize()
        {
            _mock = new Mock<GameData>();
            _model = new GameModel("Test");
        }

        [TestMethod]
        public void AquaCoasterModelNewGameTest()
        {
            _model.NewGame("Test");

            // Starting values
            Assert.AreEqual("Test", _model.Name);
            Assert.AreEqual(26, _model.Rows);
            Assert.AreEqual(26, _model.Columns);
            Assert.AreEqual(100000, _model.Budget);
            Assert.AreEqual(0, _model.GameTime);
            Assert.AreEqual(1000, _model.TimeInterval);
            Assert.AreEqual(AquaCoaster.Model.Enums.ParkStatus.CLOSED, _model.ParkStatus);

            Assert.AreEqual(0, _model.VisitorCount);
        }

        [TestMethod]
        public void AquaCoasterModelNewGameButDoNotStartTest()
        {
            _model.NewGameButDoNotStart("Test");

            // Starting values
            Assert.AreEqual("Test", _model.Name);
            Assert.AreEqual(26, _model.Rows);
            Assert.AreEqual(26, _model.Columns);
            Assert.AreEqual(100000, _model.Budget);
            Assert.AreEqual(0, _model.GameTime);
            Assert.AreEqual(1000, _model.TimeInterval);
            Assert.AreEqual(AquaCoaster.Model.Enums.ParkStatus.CLOSED, _model.ParkStatus);

            Assert.AreEqual(0, _model.VisitorCount);
        }

        [TestMethod]
        public void AquaCoasterModelOpenCloseParkTest()
        {
            _model.NewGame("Test");

            _model.OpenPark();
            Assert.AreEqual(AquaCoaster.Model.Enums.ParkStatus.OPEN, _model.ParkStatus);

            _model.ClosePark();
            Assert.AreEqual(AquaCoaster.Model.Enums.ParkStatus.CLOSED, _model.ParkStatus);
        }

        [TestMethod]
        public void AquaCoasterModelAdvanceTimeTest()
        {
            _model.NewGame("Test");

            _model.AdvanceTime();
            Assert.AreEqual(1000, _model.GameTime);
        }

        [TestMethod]
        public void AquaCoasterModelGetInfrastructureAtTest()
        {
            _model.NewGame("Test");

            Assert.AreEqual(typeof(Dirt), _model.GetInfrastructureAt(new Point(0, 0)).GetType());

            _model.Place(new Pavement(), new Point(0, 0));
            _model.Place(new Carousel(), new Point(1, 1));
            _model.Place(new Water(), new Point(0, 1));

            Assert.AreEqual(typeof(Pavement), _model.GetInfrastructureAt(new Point(0, 0)).GetType());
            Assert.AreEqual(typeof(Carousel), _model.GetInfrastructureAt(new Point(1, 1)).GetType());
            Assert.AreEqual(typeof(Carousel), _model.GetInfrastructureAt(new Point(2, 2)).GetType());
            Assert.AreEqual(typeof(Water), _model.GetInfrastructureAt(new Point(0, 1)).GetType());

            _model.Place(new Pier(), new Point(0, 1));

            Assert.AreEqual(typeof(Pier), _model.GetInfrastructureAt(new Point(0, 1)).GetType());
        }

        [TestMethod]
        public void AquaCoasterModelCanPlaceTest()
        {
            _model.NewGame("Test");

            _model.Place(new Pavement(), new Point(0, 0));
            _model.Place(new Carousel(), new Point(1, 0));

            Assert.IsTrue(_model.CanPlace(new Visitor(), new Point(0, 0)));
            Assert.IsTrue(_model.CanPlace(new Visitor(), new Point(1, 0)));
            Assert.IsTrue(_model.CanPlace(new Visitor(), new Point(1, 1)));

            Assert.IsFalse(_model.CanPlace(new Visitor(), new Point(0, 1)));
        }

        [TestMethod]
        public void AquaCoasterModelPlaceTest()
        {
            _model.NewGame("Test");

            _model.Place(new Pavement(), new Point(0, 0));

            Visitor v = new Visitor();
            _model.Place(v, new Point(0, 0));
        }

        [TestMethod]
        public void AquaCoasterModelRemovePersonTest()
        {
            _model.NewGame("Test");

            Visitor v = new Visitor();
            _model.Place(v, new Point(10, 24));
            Assert.AreEqual(1, _model.VisitorCount);

            _model.RemovePerson(v);
            Assert.AreEqual(0, _model.VisitorCount);
        }

        [TestMethod]
        public void AquaCoasterModelMovePersonToTest()
        {
            _model.NewGame("Test");

            Visitor v = new Visitor();
            Carousel c = new Carousel();
            Pavement p = new Pavement();
            _model.Place(c, new Point(0, 0));
            _model.Place(p, new Point(10, 10));

            try
            {
                _model.MovePersonTo(v, c, p, null);
                Assert.Fail();
            }
            catch (InvalidOperationException) { }
        }

        [TestMethod]
        public void AquaCoasterModelIsValidStackingTest()
        {
            _model.NewGame("Test");

            // Empty park
            Assert.IsTrue(_model.IsValidStacking(new Dirt(), new Point(0, 0)));
            Assert.IsTrue(_model.IsValidStacking(new Water(), new Point(0, 0)));
            Assert.IsTrue(_model.IsValidStacking(new Gate(), new Point(0, 0)));
            Assert.IsTrue(_model.IsValidStacking(new Pavement(), new Point(0, 0)));
            Assert.IsTrue(_model.IsValidStacking(new Carousel(), new Point(0, 0)));
            Assert.IsTrue(_model.IsValidStacking(new Coffee(), new Point(0, 0)));
            Assert.IsTrue(_model.IsValidStacking(new Plant(), new Point(0, 0)));

            _model.Place(new Water(), new Point(0, 0));

            // Valid stackings
            Assert.IsTrue(_model.IsValidStacking(new Pier(), new Point(0, 0)));
            Assert.IsTrue(_model.IsValidStacking(new Pavement(), new Point(0, 1)));
            Assert.IsTrue(_model.IsValidStacking(new Carousel(), new Point(1, 1)));

            // Invalid stackings
            Assert.IsFalse(_model.IsValidStacking(new Pavement(), new Point(0, 0)));
            Assert.IsFalse(_model.IsValidStacking(new Carousel(), new Point(0, 0)));

            _model.Place(new Carousel(), new Point(1, 1));
            Assert.IsFalse(_model.IsValidStacking(new Plant(), new Point(1, 1)));
        }

        [TestMethod]
        public void AquaCoasterModelCanPlaceInfrastructureTest()
        {
            _model.NewGame("Test");

            // Checking the corners
            Assert.IsTrue(_model.CanPlace(new Carousel(), new Point(0, 0)));
            Assert.IsFalse(_model.CanPlace(new Carousel(), new Point(0, _model.Columns - 1)));
            Assert.IsFalse(_model.CanPlace(new Carousel(), new Point(_model.Rows - 1, 0)));
            Assert.IsFalse(_model.CanPlace(new Carousel(), new Point(_model.Rows - 1, _model.Columns - 1)));
        }

        [TestMethod]
        public void AquaCoasterModelCanPlaceInfrastructureWithTypeTest()
        {
            _model.NewGame("Test");

            // Checking the corners
            Assert.IsTrue(_model.CanPlaceInfrastructureWithType(typeof(Carousel), new Point(6, 6), new Point(0, 0)));
            Assert.IsFalse(_model.CanPlaceInfrastructureWithType(typeof(Carousel), new Point(6, 6), new Point(0, _model.Columns - 1)));
            Assert.IsFalse(_model.CanPlaceInfrastructureWithType(typeof(Carousel), new Point(6, 6), new Point(_model.Rows - 1, 0)));
            Assert.IsFalse(_model.CanPlaceInfrastructureWithType(typeof(Carousel), new Point(6, 6), new Point(_model.Rows - 1, _model.Columns - 1)));

            // Stacking
            _model.Place(new Water(), new Point(0, 0));

            Assert.IsTrue(_model.CanPlaceInfrastructureWithType(typeof(Pier), new Point(1, 1), new Point(0, 0)));
            Assert.IsTrue(_model.CanPlaceInfrastructureWithType(typeof(Carousel), new Point(6, 6), new Point(1, 1)));
            Assert.IsFalse(_model.CanPlaceInfrastructureWithType(typeof(Carousel), new Point(6, 6), new Point(0, 0)));
            Assert.IsFalse(_model.CanPlaceInfrastructureWithType(typeof(Plant), new Point(1, 1), new Point(0, 0)));

            _model.Place(new Pavement(), new Point(1, 1));

            Assert.IsFalse(_model.CanPlaceInfrastructureWithType(typeof(Carousel), new Point(6, 6), new Point(1, 1)));
        }

        [TestMethod]
        public void AquaCoasterModelPlaceInfrastructureTest()
        {
            _model.NewGame("Test");

            // Right placings
            _model.Place(new Pavement(), new Point(0, 0));
            Assert.AreEqual(typeof(Pavement), _model.GetInfrastructureAt(new Point(0, 0)).GetType());
            Assert.AreEqual(99900, _model.Budget);

            _model.Place(new Carousel(), new Point(1, 1));
            Assert.AreEqual(typeof(Carousel), _model.GetInfrastructureAt(new Point(1, 1)).GetType());
            Assert.AreEqual(typeof(Carousel), _model.GetInfrastructureAt(new Point(2, 2)).GetType());
            Assert.AreEqual(98900, _model.Budget);

            _model.Place(new Water(), new Point(0, 1));
            Assert.AreEqual(typeof(Water), _model.GetInfrastructureAt(new Point(0, 1)).GetType());
            Assert.AreEqual(98900, _model.Budget);

            _model.Place(new Pier(), new Point(0, 1));
            Assert.AreEqual(typeof(Pier), _model.GetInfrastructureAt(new Point(0, 1)).GetType());
            Assert.AreEqual(98700, _model.Budget);

            // Wrong placing
            _model.Place(new Plant(), new Point(1, 1));
            Assert.AreEqual(typeof(Carousel), _model.GetInfrastructureAt(new Point(1, 1)).GetType());
            Assert.AreEqual(98700, _model.Budget);
        }

        [TestMethod]
        public void AquaCoasterModelPlaceAllTest()
        {
            _model.NewGame("Test");

            List<Tuple<Infrastructure, Point>> infs = new List<Tuple<Infrastructure, Point>>();
            // The table is initialized with dirt, so the testing is with plants
            infs.Add(new Tuple<Infrastructure, Point>(new Plant(), new Point(0, 0)));
            infs.Add(new Tuple<Infrastructure, Point>(new Plant(), new Point(1, 1)));
            infs.Add(new Tuple<Infrastructure, Point>(new Plant(), new Point(3, 6)));
            infs.Add(new Tuple<Infrastructure, Point>(new Plant(), new Point(_model.Rows - 1, _model.Columns - 1)));

            _model.PlaceAll(infs);

            Assert.AreEqual(typeof(Plant), (_model.GetInfrastructureAt(new Point(0, 0)).GetType()));
            Assert.AreEqual(typeof(Plant), (_model.GetInfrastructureAt(new Point(1, 1)).GetType()));
            Assert.AreEqual(typeof(Plant), (_model.GetInfrastructureAt(new Point(3, 6)).GetType()));
            Assert.AreEqual(typeof(Plant), (_model.GetInfrastructureAt(new Point(_model.Rows - 1, _model.Columns - 1)).GetType()));
        }

        [TestMethod]
        public void AquaCoasterModelCanDemolishTest()
        {
            _model.NewGame("Test");

            // Cannot demolish
            Assert.IsFalse(_model.CanDemolish(new Point(0, 0)));
            Assert.IsFalse(_model.CanDemolish(new Point(10, 24)));

            // Can demoolish
            _model.Place(new Carousel(), new Point(0, 0));
            Assert.IsTrue(_model.CanDemolish(new Point(0, 0)));

            _model.Place(new Carousel(), new Point(0, 0));
            Assert.IsTrue(_model.CanDemolish(new Point(1, 1)));
        }

        [TestMethod]
        public void AquaCoasterModelDemolishtTest()
        {
            _model.NewGame("Test");

            _model.Place(new Carousel(), new Point(0, 0));
            _model.Demolish(new Point(0, 0));

            Assert.AreEqual(typeof(Dirt), _model.GetInfrastructureAt(new Point(0, 0)).GetType());

            // Demolishing the gate
            _model.Demolish(new Point(10, 24));

            Assert.AreEqual(typeof(Gate), _model.GetInfrastructureAt(new Point(10, 24)).GetType());
        }
    }
}


