namespace ChordsSender.Tests
{
    [TestFixture]
    public class GreeterTests
    {

        [Test]
        public void GetGreeting_Morning_Returns_GoodMorning()
        {
            for(int hour = 0; hour <= 11; hour++)
            {
                // Arrange
                var greeter = new Greeter();

                // Act
                var result = greeter.GetGreeting(hour);

                // Assert
                Assert.That(result, Is.EqualTo("Good morning po team. Ito po ang ating chords para po ngayong Linggo. See you all! :)"));
            }
        }

        [Test]
        public void GetGreeting_Morning_Returns_GoodAfternoon()
        {
            for (int hour = 12; hour <= 17; hour++)
            {
                // Arrange
                var greeter = new Greeter();

                // Act
                var result = greeter.GetGreeting(hour);

                // Assert
                Assert.That(result, Is.EqualTo("Good afternoon po team. Ito po ang ating chords para po ngayong Linggo. See you all! :)"));
            }
        }

        [Test]
        public void GetGreeting_Morning_Returns_GoodEvening()
        {
            for (int hour = 18; hour <= 23; hour++)
            {
                // Arrange
                var greeter = new Greeter();

                // Act
                var result = greeter.GetGreeting(hour);

                // Assert
                Assert.That(result, Is.EqualTo("Good evening po team. Ito po ang ating chords para po ngayong Linggo. See you all! :)"));
            }
        }
    }
}