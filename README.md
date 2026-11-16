# Vienībtestēšana — Habit Manager

## Kā palaist

```bash
dotnet test HabitManager.Tests
dotnet test HabitManager.Tests --collect:"XPlat Code Coverage"   
```

## Kas tiek testēts

| Fails | Ko testē | Atkarības |
|---|---|---|
| [HabitTests.cs](HabitTests.cs) | domēna klase `Habit`: validācija, grafiks, izpildes atzīmēšana, labošana, arhivēšana | nav |
| [WeekDaysTests.cs](WeekDaysTests.cs) | `WeekDays` karodziņi un `DayOfWeek` pārveide | nav |
| [HabitServiceTests.cs](HabitServiceTests.cs) | aplikācijas slāņa `HabitService` | `IHabitRepository` aizstāts ar Moq, laiks ar `FixedTimeProvider` |
| [HabitMapperTests.cs](HabitMapperTests.cs), [HabitDtoTests.cs](HabitDtoTests.cs) | pārveide domēns → DTO, DTO aprēķinātie lauki | nav |
| [HabitApiClientTests.cs](HabitApiClientTests.cs) | Maui API klients | `HttpClient` ar [StubHttpMessageHandler](StubHttpMessageHandler.cs) |
| [TestDataFactoryTests.cs](TestDataFactoryTests.cs) | testa dati ir derīgi un daudzveidīgi | nav |

## Identificētie principi un kā tie parādās risinājumā

### 1. Viens tests pārbauda vienu lietu 

Ja viena darbība maina vairākas lietas, katrai ir savs tests, lai kļūdas gadījumā uzreiz redzams, kas tieši nestrādā. `HabitService.UpdateAsync` ir pārbaudīts divos testos: viens pārbauda atgriezto rezultātu, otrs, ka izmaiņas tiek saglabātas ([HabitServiceTests.cs](HabitServiceTests.cs)):

```csharp
public async Task UpdateAsync_ValidRequest_ReturnsUpdatedHabit()  { ... Assert.Equal(NewName, result.Name); }
public async Task UpdateAsync_ValidRequest_SavesChanges()         { ... repository.Verify(r => r.UpdateAsync(habit), Times.Once); }
```

Tāpat `Archive_ActiveHabit_SetsArchivedStatus` un `Archive_ActiveHabit_DisablesEditing` ir atsevišķi testi ([HabitTests.cs](HabitTests.cs)).

### 2. Testu nosaukumi: metode, scenārijs, gaidāmais rezultāts 

Visi nosaukumi ir formā `Metode_Scenārijs_GaidāmaisRezultāts`:

```csharp
Constructor_NameLongerThanMaximum_ThrowsArgumentException
MarkCompleted_SameDateTwice_ThrowsInvalidOperationException
CompleteAsync_FutureDate_ThrowsArgumentException
GetAsync_HabitBelongsToOtherUser_ThrowsNotFoundException
```

Kad tests neizdodas, nosaukums pasaka, kurš scenārijs nestrādā.

### 3. Arrange – Act – Assert 

Katrs tests ir sadalīts trīs skaidri atdalītās daļās ar komentāriem ([HabitTests.cs](HabitTests.cs)):

```csharp
[Fact]
public void MarkCompleted_ScheduledDay_RecordsCompletionForThatDate()
{
    // Arrange
    var habit = CreateHabit();

    // Act
    var completion = habit.MarkCompleted(Monday);

    // Assert
    Assert.Equal(Monday, completion.Date);
}
```

### 4. Nav "burvju" vērtību 

Testu vērtībām ir nosaukumi: `UserId`, `HabitName`, `NewName`, `MaxNameLength`, `NotFoundDetail`. Piemēram, robežvērtību tests nesatur skaitli bez skaidrojuma:

```csharp
private const int MaxNameLength = 100;
...
var tooLongName = new string('a', MaxNameLength + 1);
```

Datumi `Monday`/`Tuesday`/`Today` ir nosaukti lasāmi, un komentāri paskaidro, ka, piemēram, `Today` ir pirmdiena.

### 5. Loģika testos ir izvairīta 

Testos nav `if`, `for` vai `switch`. 

```csharp
Assert.All(habits, habit =>
    Assert.All(habit.Completions, completion =>
        Assert.True(habit.IsScheduledOn(completion.Date))));
```

### 6. Palīgmetodes, nevis kopīga Setup/Teardown 

```csharp
private static Habit CreateHabit(string userId = UserId, WeekDays days = WeekDays.All, string name = HabitName) => ...
private static HabitService CreateService(Mock<IHabitRepository> repository) => ...
private static Mock<IHabitRepository> RepositoryReturning(params Habit[] habits) { ... }
```

### 7. Viena darbība uz testu, parametrizēti testi 

Kur jāpārbauda vairāki ievaddati vienai un tai pašai darbībai, izmantots `[Theory]` ar `[InlineData]`. Katra vērtība kļūst par atsevišķu testu un, ja neizdodas ir skaidrs, kura ([HabitTests.cs](HabitTests.cs)):

```csharp
[Theory]
[InlineData(0, true)]   // pirmdiena
[InlineData(1, false)]  // otrdiena
[InlineData(2, true)]   // trešdiena
...
public void IsScheduledOn_MondayWednesdayFridaySchedule_MatchesOnlyScheduledDays(int daysAfterMonday, bool expected)
```

### 8. Statiskās atsauces tiek aizstātas ar "šuvi" 

`HabitService.CompleteAsync` nosaka "šodienu" un noraida nākotnes datumus. Lai testam būtu pilna kontrole pār laiku, serviss nelieto `DateTime.Today`, bet `TimeProvider` ([HabitService.cs](../HabitManager.Application/HabitService.cs)):

```csharp
public class HabitService(IHabitRepository repository, TimeProvider? timeProvider = null)
{
    private readonly TimeProvider _time = timeProvider ?? TimeProvider.System;
    ...
    var today = _time.GetLocalNow().Date;
```

Produkcijā tiek lietots `TimeProvider.System`, testos [FixedTimeProvider](FixedTimeProvider.cs), kas vienmēr atgriež vienu datumu. Tāpēc testi `CompleteAsync_NoDateGiven_CompletesHabitForToday` un `CompleteAsync_FutureDate_ThrowsArgumentException` dod to pašu rezultātu jebkurā dienā.

### 9. Vāji saistīta arhitektūra, interfeisi un imitācijas objekti 

`HabitService` atkarīgs no `IHabitRepository`, nevis no konkrētas datubāzes, tāpēc testos repozitorijs tiek aizstāts ar Moq. Slaidā 15 minētā atšķirība starp stub un mock ir ievērota:

```csharp
// stub: kontrolē, ko atgriež atkarība (netiek pārbaudīts)
repository.Setup(r => r.GetByIdAsync(habit.Id)).ReturnsAsync(habit);

// mock: pārbauda, vai serviss atkarību izsauca
repository.Verify(r => r.AddAsync(It.Is<Habit>(h => h.UserId == UserId && h.Name == HabitName)), Times.Once);
repository.Verify(r => r.UpdateAsync(It.IsAny<Habit>()), Times.Never);   // nekas netiek saglabāts kļūdas gadījumā
```

### 10. Izņēmumu testēšana 

Izņēmumi tiek pārbaudīti ar `Assert.Throws<T>` / `Assert.ThrowsAsync<T>` un pārbaudīts tiek izņēmuma **veids**, nevis teksts, jo teksti mainās un tādi testi kļūst trausli:

```csharp
await Assert.ThrowsAsync<NotFoundException>(() => service.GetAsync(UserId, unknownId));
```

### 11. Validācijas testēšana 

```csharp
Assert.Throws<ArgumentException>(() => habit.Update(string.Empty, NewNote, Frequency.Monthly, WeekDays.Friday));

Assert.Equal(ValidName, habit.Name);
Assert.Equal(WeekDays.Monday, habit.ScheduledDays);
```

### 12. Privātās metodes netiek testētas tieši 

`Habit.ValidateName`, `Habit.ValidateDays` un `HabitService.GetOwnedAsync` ir privātas. Tās tiek pārbaudītas caur publiskajām metodēm, kuras tās izsauc, piemēram, `GetAsync_HabitBelongsToOtherUser_ThrowsNotFoundException` pārbauda `GetOwnedAsync` loģiku, aprakstot tikai redzamo uzvedību.

### 13. Asinhrono operāciju testēšana 

Servisa metodes ir asinhronas un izmanto repozitoriju. Testi ir `async Task` metodes ar `await`, bet repozitorijs ir imitēts ar `ReturnsAsync(...)`, tāpēc testā nav reālas gaidīšanas.

### 14. Seguma procents nav mērķis 

Segumu var izmērīt (`--collect:"XPlat Code Coverage"`), bet testi nav rakstīti, lai sasniegtu skaitli.
