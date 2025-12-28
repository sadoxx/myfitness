-- Add WeightLogs table for progress tracking
CREATE TABLE IF NOT EXISTS "WeightLogs" (
    "Id" uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId" uuid NOT NULL,
    "Weight" numeric(5,2) NOT NULL,
    "LogDate" timestamp with time zone NOT NULL,
    "Notes" text,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL DEFAULT false,
    CONSTRAINT "FK_WeightLogs_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users"("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_WeightLogs_UserId" ON "WeightLogs" ("UserId");
CREATE INDEX IF NOT EXISTS "IX_WeightLogs_LogDate" ON "WeightLogs" ("LogDate");
CREATE INDEX IF NOT EXISTS "IX_WeightLogs_IsDeleted" ON "WeightLogs" ("IsDeleted");
