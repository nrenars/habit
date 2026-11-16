CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);

BEGIN TRANSACTION;
CREATE TABLE "Habits" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_Habits" PRIMARY KEY,
    "UserId" TEXT NOT NULL,
    "Name" TEXT NOT NULL,
    "Note" TEXT NOT NULL,
    "Status" TEXT NOT NULL,
    "Frequency" TEXT NOT NULL,
    "ScheduledDays" INTEGER NOT NULL,
    "CreatedAt" TEXT NOT NULL,
    "EditedAt" TEXT NOT NULL
);

CREATE TABLE "HabitCompletions" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_HabitCompletions" PRIMARY KEY,
    "HabitId" TEXT NOT NULL,
    "Date" TEXT NOT NULL,
    CONSTRAINT "FK_HabitCompletions_Habits_HabitId" FOREIGN KEY ("HabitId") REFERENCES "Habits" ("Id") ON DELETE CASCADE
);

CREATE UNIQUE INDEX "IX_HabitCompletions_HabitId_Date" ON "HabitCompletions" ("HabitId", "Date");

CREATE INDEX "IX_Habits_UserId" ON "Habits" ("UserId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261115152014_InitialCreate', '10.0.0');

COMMIT;

