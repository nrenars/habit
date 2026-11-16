# Vienībtestēšana — Habit Manager

Šajā mapē ir vienībtesti ieradumu pārvaldnieka risinājumam (xUnit + Moq). README apraksta, kādi vienībtestēšanas principi identificēti 12. lekcijas materiālā ("Unit Testing") un kā tie parādās kodā.

## Kā palaist

```bash
dotnet test HabitManager.Tests
dotnet test HabitManager.Tests --collect:"XPlat Code Coverage"   
```

Testi neprasa ne datubāzi, ne serveri, ne failus, ne tīklu.

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

### 1. Viens tests pārbauda vienu lietu (slaids 10, 22)

Ja viena darbība maina vairākas lietas, katrai ir savs tests, lai kļūdas gadījumā uzreiz redzams, kas tieši nestrādā. `HabitService.UpdateAsync` ir pārbaudīts divos testos: viens pārbauda atgriezto rezultātu, otrs, ka izmaiņas tiek saglabātas ([HabitServiceTests.cs](HabitServiceTests.cs)):

```csharp
public async Task UpdateAsync_ValidRequest_ReturnsUpdatedHabit()  { ... Assert.Equal(NewName, result.Name); }
public async Task UpdateAsync_ValidRequest_SavesChanges()         { ... repository.Verify(r => r.UpdateAsync(habit), Times.Once); }
```

Tāpat `Archive_ActiveHabit_SetsArchivedStatus` un `Archive_ActiveHabit_DisablesEditing` ir atsevišķi testi ([HabitTests.cs](HabitTests.cs)).

### 2. Labs vienībtests ir ātrs, izolēts, atkārtojams, pats sevi pārbauda un tiek uzrakstīts savlaicīgi (slaids 11)

- **Fast un Isolated:** nav datubāzes, failu sistēmas un tīkla. Repozitorijs ir Moq objekts, HTTP klients saņem stub apdarinātāju, tāpēc visi testi izpildās milisekundēs.
- **Repeatable:** datumi ir fiksēti (`new DateTime(2026, 11, 16)`), nevis `DateTime.Now`, un laiks serviša testos tiek kontrolēts (skat. 9. punktu). Testi dod to pašu rezultātu jebkurā dienā.
- **Self-checking:** katrs tests beidzas ar `Assert`, un nekas nav jāpārbauda ar roku.
- **Timely:** testi nav sarežģītāki par kodu, ko tie pārbauda. Ja kāda klase būtu grūti testējama, tas būtu signāls mainīt dizainu (kā ar laiku, skat. 9. punktu).

### 3. Testu nosaukumi: metode, scenārijs, gaidāmais rezultāts (slaids 17)

Visi nosaukumi ir formā `Metode_Scenārijs_GaidāmaisRezultāts`:

```csharp
Constructor_NameLongerThanMaximum_ThrowsArgumentException
MarkCompleted_SameDateTwice_ThrowsInvalidOperationException
CompleteAsync_FutureDate_ThrowsArgumentException
GetAsync_HabitBelongsToOtherUser_ThrowsNotFoundException
```

Kad tests nokrīt, nosaukums pasaka, kurš scenārijs nestrādā.

### 4. Arrange – Act – Assert (slaids 18)

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

Testiem, kas pārbauda izņēmumu, "Act" un "Assert" ir apvienoti vienā `Assert.Throws` rindā (`// Act + Assert`).

### 5. Nav "burvju" vērtību (slaids 19)

Testu vērtībām ir nosaukumi: `UserId`, `HabitName`, `NewName`, `MaxNameLength`, `NotFoundDetail`. Piemēram, robežvērtību tests nesatur skaitli bez skaidrojuma:

```csharp
private const int MaxNameLength = 100;
...
var tooLongName = new string('a', MaxNameLength + 1);
```

Datumi `Monday`/`Tuesday`/`Today` ir nosaukti lasāmi, un komentāri paskaidro, ka `Today` ir pirmdiena.

### 6. Loģika testos ir izvairīta (slaids 20)

Testos nav `if`, `for` vai `switch`. Kur vajag daudz ievaddatu, izmantoti `[Theory]` un `Assert.All`, nevis cikli:

```csharp
Assert.All(habits, habit =>
    Assert.All(habit.Completions, completion =>
        Assert.True(habit.IsScheduledOn(completion.Date))));
```

Cikls ir tikai palīgmetodē `RepositoryReturning`, kas sagatavo atkarību, nevis pašā testā.

### 7. Palīgmetodes, nevis kopīga Setup/Teardown (slaids 21, 45)

xUnit katram testam izveido jaunu klases instanci, un testi nelieto kopīgu stāvokli. Kopīgā sagatavošana ir palīgmetodēs, ko katrs tests izsauc pats, tāpēc visa tā nepieciešamā sagatavošana ir redzama testa iekšienē:

```csharp
private static Habit CreateHabit(string userId = UserId, WeekDays days = WeekDays.All, string name = HabitName) => ...
private static HabitService CreateService(Mock<IHabitRepository> repository) => ...
private static Mock<IHabitRepository> RepositoryReturning(params Habit[] habits) { ... }
```

### 8. Viena darbība uz testu, parametrizēti testi (slaids 22, 30–33)

Kur jāpārbauda vairāki ievaddati vienai un tai pašai darbībai, izmantots `[Theory]` ar `[InlineData]`. Katra vērtība kļūst par atsevišķu testu un, ja nokrīt, ir skaidrs, kura ([HabitTests.cs](HabitTests.cs)):

```csharp
[Theory]
[InlineData(0, true)]   // pirmdiena
[InlineData(1, false)]  // otrdiena
[InlineData(2, true)]   // trešdiena
...
public void IsScheduledOn_MondayWednesdayFridaySchedule_MatchesOnlyScheduledDays(int daysAfterMonday, bool expected)
```

Tāpat ar `[Theory]` pārbaudīti tukši/atstarpju nosaukumi, nederīgi nedēļas dienu kodi un visas septiņas `DayOfWeek` → `WeekDays` pārveides ([WeekDaysTests.cs](WeekDaysTests.cs)). `[Fact]` izmantots fiksētiem scenārijiem.

### 9. Statiskās atsauces tiek aizstātas ar "šuvi" (slaids 24)

`HabitService.CompleteAsync` nosaka "šodienu" un noraida nākotnes datumus. Lai testam būtu pilna kontrole pār laiku, serviss nelieto `DateTime.Today`, bet `TimeProvider` ([HabitService.cs](../HabitManager.Application/HabitService.cs)):

```csharp
public class HabitService(IHabitRepository repository, TimeProvider? timeProvider = null)
{
    private readonly TimeProvider _time = timeProvider ?? TimeProvider.System;
    ...
    var today = _time.GetLocalNow().Date;
```

Produkcijā tiek lietots `TimeProvider.System`, testos [FixedTimeProvider](FixedTimeProvider.cs), kas vienmēr atgriež vienu datumu. Tāpēc testi `CompleteAsync_NoDateGiven_CompletesHabitForToday` un `CompleteAsync_FutureDate_ThrowsArgumentException` dod to pašu rezultātu jebkurā dienā.

Domēna klase `Habit` vairumā metožu saņem datumu kā parametru (`MarkCompleted(date)`, `IsScheduledOn(date)`), tāpēc arī tās ir testējamas ar fiksētiem datumiem. Tikai `CanCompleteToday` iekšēji lieto `DateTime.Today`. Šos trīs testus netraucē datuma maiņa, jo ieradumam ir grafiks "visas dienas", bet tie tomēr ir vienīgie testi ar slēptu atkarību no pulksteņa. Tā ir zināma atlikusī nepilnība.

### 10. Vāji saistīta arhitektūra, interfeisi un imitācijas objekti (slaids 12–15, 46)

`HabitService` atkarīgs no `IHabitRepository`, nevis no konkrētas datubāzes, tāpēc testos repozitorijs tiek aizstāts ar Moq. Slaidā 15 minētā atšķirība starp stub un mock ir ievērota:

```csharp
// stub: kontrolē, ko atgriež atkarība (netiek pārbaudīts)
repository.Setup(r => r.GetByIdAsync(habit.Id)).ReturnsAsync(habit);

// mock: pārbauda, vai serviss atkarību izsauca
repository.Verify(r => r.AddAsync(It.Is<Habit>(h => h.UserId == UserId && h.Name == HabitName)), Times.Once);
repository.Verify(r => r.UpdateAsync(It.IsAny<Habit>()), Times.Never);   // nekas netiek saglabāts kļūdas gadījumā
```

Tas pats princips lietots Maui API klientam: `HttpClient` tiek izveidots ar [StubHttpMessageHandler](StubHttpMessageHandler.cs), kas atgriež gatavu JSON atbildi vai met savienojuma kļūdu, tāpēc tests neprasa palaistu API ([HabitApiClientTests.cs](HabitApiClientTests.cs)).

### 11. Izņēmumu testēšana (slaids 36, 52)

Izņēmumi tiek pārbaudīti ar `Assert.Throws<T>` / `Assert.ThrowsAsync<T>` un pārbaudīts tiek izņēmuma **veids**, nevis teksts, jo teksti mainās un tādi testi kļūst trausli:

```csharp
await Assert.ThrowsAsync<NotFoundException>(() => service.GetAsync(UserId, unknownId));
```

Vienīgais izņēmums ir `GetHabitAsync_ServerReturnsNotFound_ThrowsApiExceptionWithServerDetail`, kur teksts tiek pārbaudīts apzināti, jo servera ziņojums ir tieši tas, ko API klients paredz parādīt lietotājam, un tas ir klienta uzvedība, nevis implementācijas detaļa. Tests to paskaidro komentārā.

### 12. Validācijas testēšana (slaids 53–55)

Validācija ir pārbaudīta gan ar pareiziem, gan nepareiziem datiem, ieskaitot robežas: nosaukums ar 100 simboliem tiek pieņemts, ar 101 netiek; tukši un atstarpju nosaukumi, tukšs `UserId`, neviena izvēlēta diena un nederīgs dienu kods. Tests `Update_InvalidName_LeavesHabitUnchanged` pārbauda ne tikai to, ka izņēmums tiek mests, bet arī to, ka ieradums pēc tam ir palicis neizmainīts:

```csharp
Assert.Throws<ArgumentException>(() => habit.Update(string.Empty, NewNote, Frequency.Monthly, WeekDays.Friday));

Assert.Equal(ValidName, habit.Name);
Assert.Equal(WeekDays.Monday, habit.ScheduledDays);
```

### 13. Privātās metodes netiek testētas tieši (slaids 23)

`Habit.ValidateName`, `Habit.ValidateDays` un `HabitService.GetOwnedAsync` ir privātas. Tās tiek pārbaudītas caur publiskajām metodēm, kuras tās izsauc, piemēram, `GetAsync_HabitBelongsToOtherUser_ThrowsNotFoundException` pārbauda `GetOwnedAsync` loģiku, aprakstot tikai redzamo uzvedību.

### 14. Asinhrono operāciju testēšana (slaids 48)

Servisa metodes ir asinhronas un izmanto repozitoriju. Testi ir `async Task` metodes ar `await`, bet repozitorijs ir imitēts ar `ReturnsAsync(...)`, tāpēc testā nav reālas gaidīšanas.

### 15. Seguma procents nav mērķis (slaids 25)

Segumu var izmērīt (`--collect:"XPlat Code Coverage"`), bet testi nav rakstīti, lai sasniegtu skaitli. Tie ir rakstīti pa uzvedībām: kas notiek pareizā scenārijā, kas notiek kļūdas gadījumā un kas notiek uz robežas.

### 16. Testa dati kā regresijas aizsardzība

Iepriekšējā mājasdarba komentārā bija norādīts, ka trūkst testa datu. [TestDataFactoryTests.cs](TestDataFactoryTests.cs) pārbauda, ka dati, ko API ielādē tukšā datubāzē, satur vairākus lietotājus, arhivētu ieradumu, ieradumu ar izvēlētām nedēļas dienām, izpildes vēsturi un ka izpildes ieraksti nekad nav ierakstīti dienās, kas nav ieraduma grafikā.

## Izmaiņas produkcijas kodā testējamības dēļ

Vienīgā izmaiņa ir `TimeProvider` ievadīšana `HabitService` (9. punkts). Parametrs ir neobligāts un noklusēti lieto `TimeProvider.System`, tāpēc API reģistrācija un uzvedība nemainās.

## Kas netiek vienībtestēts un kāpēc

- **`EfHabitRepository` un datubāze:** vienībtests nedrīkst būt atkarīgs no datubāzes (slaids 11). Datu saglabāšana un unikālais indekss `(HabitId, Date)` ir pārbaudīti manuāli ar `curl` pret reālu API (dublikāta atzīmēšana atgriež `409`). To pareizi segtu atsevišķi integrācijas testi.
- **API endpointi:** tie ir plāna adaptera kārta, kas izsauc `HabitService`. Tos segtu integrācijas testi ar `WebApplicationFactory`.
- **Maui ekrāni (`*.xaml.cs`):** lietotne izmanto code-behind bez ViewModel klasēm, tāpēc nav kā testēt UI loģiku atsevišķi no platformas.

## Kas no slaidiem netika pielietots

- **TDD (slaids 56–57):** testi tika rakstīti pēc koda, nevis pirms tā.
- **MVVM, `INotifyPropertyChanged`, `MessagingCenter` (slaids 47–51):** šajā risinājumā nav view model klašu, tāpēc šie testu veidi nav piemērojami.
