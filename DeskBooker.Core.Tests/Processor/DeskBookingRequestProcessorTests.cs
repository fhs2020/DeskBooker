using DeskBooker.Core.DataInterface;
using DeskBooker.Core.Domain;

using Moq;

using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;

namespace DeskBooker.Core.Processor
{
    public class DeskBookingRequestProcessorTests
    {
        private readonly DeskBookingRequest _request;
        private readonly List<Desk> _availableDesks;
        private readonly Mock<IDeskBookingRepository> _deskbookingRepositoryMock;
        private readonly Mock<IDeskRepository> _deskRepositoryMock;
        private readonly DeskBookingRequestProcessor _processor;

        public DeskBookingRequestProcessorTests()
        {
            _request = new DeskBookingRequest
            {
                FirstName = "Thomas",
                LastName = "Huber",
                Email = "thomas@thoamsclaudiushuber.com",
                Date = new DateTime(2020, 01, 28)
            };

            _availableDesks = new List<Desk> { new Desk { Id = 7 } };
            _deskbookingRepositoryMock = new Mock<IDeskBookingRepository>();
            _deskRepositoryMock = new Mock<IDeskRepository>();
            _deskRepositoryMock.Setup(x => x.GetAvailableDesks(_request.Date))
                .Returns(_availableDesks);

            _processor = new DeskBookingRequestProcessor(_deskbookingRepositoryMock.Object, _deskRepositoryMock.Object);
        }


        [Fact]
        public void ShouldReturnDeskBookingResultWithRequestValues()
        {
            // Arrange



            // Act
            DeskbookingResult result = _processor.BookDesk(_request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(_request.FirstName, result.FirstName);
            Assert.Equal(_request.LastName, result.LastName);
            Assert.Equal(_request.Email, result.Email);
            Assert.Equal(_request.Date, result.Date);
        }

        [Fact]
        public void ShouldThrowExceptionIfRequestIsNull()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => _processor.BookDesk(null));

            Assert.Equal("request", exception.ParamName);

        }

        [Fact]
        public void ShouldSaveDeskBooking()
        {
            DeskBooking saveDeskBooking = null;
            _deskbookingRepositoryMock.Setup(x => x.Save(It.IsAny<DeskBooking>()))
                .Callback<DeskBooking>(x =>
                {
                    saveDeskBooking = x;
                });

            _processor.BookDesk(_request);
            _deskbookingRepositoryMock.Verify(x => x.Save(It.IsAny<DeskBooking>()), Times.Once());

            Assert.NotNull(saveDeskBooking);
            Assert.Equal(_request.FirstName, saveDeskBooking.FirstName);
            Assert.Equal(_request.LastName, saveDeskBooking.LastName);
            Assert.Equal(_request.Email, saveDeskBooking.Email);
            Assert.Equal(_request.Date, saveDeskBooking.Date);
            Assert.Equal(_availableDesks.First().Id, saveDeskBooking.DeskId);
        }

        [Fact]
        public void ShouldNotSaveDeskBookingIfNoDeskisAvailable()
        {
            _availableDesks.Clear();

            _processor.BookDesk(_request);
            _deskbookingRepositoryMock.Verify(x => x.Save(It.IsAny<DeskBooking>()), Times.Never());
        }

        [Theory]
        [InlineData(DeskBookingResultCode.Success, true)]
        [InlineData(DeskBookingResultCode.NoDeskAvailable, false)]
        public void ShouldReturnExpetectedResultCode(
            DeskBookingResultCode expectedResultCode, bool isDeskAvailable)
        {
            if (!isDeskAvailable)
            {
                _availableDesks.Clear();
            }

            var result = _processor.BookDesk(_request);

            Assert.Equal(expectedResultCode, result.Code);

        }

        [Theory]
        [InlineData(5, true)]
        [InlineData(null, false)]
        public void ShouldReturnExpetectedDeskBookingId(
            int? expectedDeskBoookingId, bool isDeskAvailable)
        {
            if (!isDeskAvailable)
            {
                _availableDesks.Clear();
            }
            else
            {
                _deskbookingRepositoryMock.Setup(x => x.Save(It.IsAny<DeskBooking>()))
                     .Callback<DeskBooking>(deskBooking =>
                     {
                         deskBooking.Id = expectedDeskBoookingId.Value;
                     });
            }

            var result = _processor.BookDesk(_request);

            Assert.Equal(expectedDeskBoookingId, result.DeskBookingId);

        }
    }
}
