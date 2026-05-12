

-- Add TestBy in Demographics
ALTER TABLE "demographics"
ADD COLUMN IF NOT EXISTS "TestBy" VARCHAR(100);