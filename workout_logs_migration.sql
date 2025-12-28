-- Add Workout Logging Tables

CREATE TABLE IF NOT EXISTS "WorkoutLogs" (
    "Id" uuid PRIMARY KEY,
    "UserId" uuid NOT NULL REFERENCES "Users"("Id"),
    "WorkoutDate" timestamp NOT NULL,
    "WorkoutName" text,
    "DurationMinutes" int NOT NULL,
    "Notes" text,
    "CreatedAt" timestamp NOT NULL,
    "UpdatedAt" timestamp,
    "IsDeleted" boolean NOT NULL DEFAULT false
);

CREATE TABLE IF NOT EXISTS "ExerciseLogs" (
    "Id" uuid PRIMARY KEY,
    "WorkoutLogId" uuid NOT NULL REFERENCES "WorkoutLogs"("Id") ON DELETE CASCADE,
    "ExerciseId" uuid NOT NULL REFERENCES "Exercises"("Id"),
    "OrderIndex" int NOT NULL,
    "CreatedAt" timestamp NOT NULL,
    "UpdatedAt" timestamp,
    "IsDeleted" boolean NOT NULL DEFAULT false
);

CREATE TABLE IF NOT EXISTS "SetLogs" (
    "Id" uuid PRIMARY KEY,
    "ExerciseLogId" uuid NOT NULL REFERENCES "ExerciseLogs"("Id") ON DELETE CASCADE,
    "SetNumber" int NOT NULL,
    "Reps" int NOT NULL,
    "Weight" decimal(18,2),
    "Completed" boolean NOT NULL,
    "CreatedAt" timestamp NOT NULL,
    "UpdatedAt" timestamp,
    "IsDeleted" boolean NOT NULL DEFAULT false
);

-- Add IsDeleted column to existing tables if they don't have it
ALTER TABLE "WorkoutLogs" ADD COLUMN IF NOT EXISTS "IsDeleted" boolean NOT NULL DEFAULT false;
ALTER TABLE "ExerciseLogs" ADD COLUMN IF NOT EXISTS "IsDeleted" boolean NOT NULL DEFAULT false;
ALTER TABLE "SetLogs" ADD COLUMN IF NOT EXISTS "IsDeleted" boolean NOT NULL DEFAULT false;

CREATE INDEX IF NOT EXISTS "IX_WorkoutLogs_UserId" ON "WorkoutLogs"("UserId");
CREATE INDEX IF NOT EXISTS "IX_ExerciseLogs_WorkoutLogId" ON "ExerciseLogs"("WorkoutLogId");
CREATE INDEX IF NOT EXISTS "IX_ExerciseLogs_ExerciseId" ON "ExerciseLogs"("ExerciseId");
CREATE INDEX IF NOT EXISTS "IX_SetLogs_ExerciseLogId" ON "SetLogs"("ExerciseLogId");
