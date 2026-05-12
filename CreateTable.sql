-- =============================================================
-- AI Maturity Assessment (Ascend) - full database schema
-- Lecture 3: CREATE TABLE, sequences, PRIMARY KEY, UNIQUE,
--            NOT NULL, CHECK constraints, DROP … CASCADE.
-- Lecture 7: constraints are the last line of defence - the
--            application validates first, but the database
--            refuses bad data even if the app has a bug.
--
-- Two tables:
--   appuser              - stores up to 10 login names
--   aimaturityassessment - stores assessment rows linked by username
--
-- How to (re-)run this script:
--   1. Open pgAdmin 4
--   2. Select the "aimaturitydb" database
--   3. Open Query Tool (Tools → Query Tool)
--   4. Paste this file and press F5 / Run
--   WARNING: DROP … CASCADE deletes all existing data.
-- =============================================================


-- =============================================================
-- 1. Clean slate - drop everything in the right order
--    (assessments first because it references appuser by username)
-- =============================================================

DROP TABLE    IF EXISTS public.aimaturityassessment CASCADE;
DROP SEQUENCE IF EXISTS public.aimaturityassessment_id_seq;

DROP TABLE    IF EXISTS public.appuser CASCADE;
DROP SEQUENCE IF EXISTS public.appuser_id_seq;


BEGIN;

-- =============================================================
-- 2. appuser table
--    Stores one row per logged-in user.
--    The application (UserRepository) enforces a maximum of 10 rows
--    by deleting the oldest entry (lowest id) before inserting when full.
--    username has a UNIQUE constraint so the same name cannot be
--    inserted twice (Lecture 3: UNIQUE constraint).
-- =============================================================

CREATE TABLE public.appuser (
    id       integer NOT NULL,
    username text    NOT NULL
);

-- UNIQUE index on username - prevents duplicate logins at the database
-- level even if the application logic has a bug (Lecture 7).
CREATE UNIQUE INDEX appuser_username_idx ON public.appuser (username);

-- Sequence for the appuser primary key.
-- Starts at 1, increments by 1, never wraps (Lecture 3: sequences).
CREATE SEQUENCE public.appuser_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.appuser_id_seq
    OWNED BY public.appuser.id;

-- Wire the sequence to the id column as its DEFAULT value so every
-- INSERT that omits id gets the next sequence number automatically.
ALTER TABLE ONLY public.appuser
    ALTER COLUMN id SET DEFAULT nextval('public.appuser_id_seq'::regclass);

ALTER TABLE ONLY public.appuser
    ADD CONSTRAINT appuser_pkey PRIMARY KEY (id);


-- =============================================================
-- 3. aimaturityassessment table
--    Stores one row per submitted assessment.
--    username links each assessment to a row in appuser (stored
--    as plain text for simplicity - no foreign key constraint in
--    this prototype so assessments survive if a user is evicted
--    by the 10-row cap).
--    CHECK constraints enforce valid ranges (Lecture 7: DB constraints).
-- =============================================================

CREATE TABLE public.aimaturityassessment (
    id                  integer     NOT NULL,
    username            text        NOT NULL DEFAULT '',
    companyname         text        NOT NULL,
    industry            text        NOT NULL,
    companysize         text        NOT NULL,
    digitalmaturity     integer     NOT NULL CHECK (digitalmaturity   BETWEEN 1 AND 5),
    datareadiness       integer     NOT NULL CHECK (datareadiness     BETWEEN 1 AND 5),
    currentaiusage      integer     NOT NULL CHECK (currentaiusage    BETWEEN 1 AND 5),
    processautomation   integer     NOT NULL CHECK (processautomation BETWEEN 1 AND 5),
    employeeskills      integer     NOT NULL CHECK (employeeskills    BETWEEN 1 AND 5),
    managementsupport   integer     NOT NULL CHECK (managementsupport BETWEEN 1 AND 5),
    budgetreadiness     integer     NOT NULL CHECK (budgetreadiness   BETWEEN 1 AND 5),
    totalscore          integer     NOT NULL CHECK (totalscore        BETWEEN 0 AND 100),
    maturitylevel       text        NOT NULL
);

-- Sequence for the assessment primary key (same pattern as appuser above).
CREATE SEQUENCE public.aimaturityassessment_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.aimaturityassessment_id_seq
    OWNED BY public.aimaturityassessment.id;

ALTER TABLE ONLY public.aimaturityassessment
    ALTER COLUMN id SET DEFAULT nextval('public.aimaturityassessment_id_seq'::regclass);

ALTER TABLE ONLY public.aimaturityassessment
    ADD CONSTRAINT aimaturityassessment_pkey PRIMARY KEY (id);

COMMIT;


-- =============================================================
-- 4. Test data - 10 sample records covering all three maturity levels.
--    Run this block after the schema above to seed the database
--    for development and demonstration purposes.
-- =============================================================

INSERT INTO public.appuser (username) VALUES
    ('Sebastian'),
    ('Maria'),
    ('Jonas'),
    ('Sofie'),
    ('Andreas');

INSERT INTO public.aimaturityassessment
    (username, companyname, industry, companysize,
     digitalmaturity, datareadiness, currentaiusage,
     processautomation, employeeskills, managementsupport,
     budgetreadiness, totalscore, maturitylevel)
VALUES
    ('Sebastian', 'Copenhagen Autoservice', 'Automotive & repair', 'Small (10-49 employees)',
     2, 2, 1, 1, 2, 3, 2, 30, 'Low AI Maturity'),

    ('Maria', 'Nordic Bakery ApS', 'Retail & e-commerce', 'Micro (2-9 employees)',
     1, 1, 1, 1, 1, 2, 1, 17, 'Low AI Maturity'),

    ('Jonas', 'Buildex Construction', 'Construction & trades', 'Small (10-49 employees)',
     2, 3, 2, 2, 2, 2, 2, 34, 'Low AI Maturity'),

    ('Sofie', 'Bright Legal', 'Professional services (legal, accounting, consulting)', 'Small (10-49 employees)',
     3, 4, 3, 3, 3, 4, 3, 64, 'Medium AI Maturity'),

    ('Andreas', 'HealthFirst Clinic', 'Healthcare & wellness clinics', 'Small (10-49 employees)',
     4, 3, 3, 3, 4, 4, 3, 68, 'Medium AI Maturity'),

    ('Sebastian', 'FitLife Studio', 'Beauty, fitness & personal care', 'Micro (2-9 employees)',
     3, 3, 2, 3, 3, 3, 3, 54, 'Medium AI Maturity'),

    ('Maria', 'CreativeHub', 'Creative & media (design, advertising, marketing)', 'Micro (2-9 employees)',
     4, 4, 4, 3, 4, 4, 4, 78, 'High AI Maturity'),

    ('Jonas', 'LogiFlow Logistics', 'Logistics, transport & courier', 'Medium (50-249 employees)',
     5, 4, 4, 5, 4, 5, 4, 88, 'High AI Maturity'),

    ('Sofie', 'TechEdge IT Services', 'IT & tech services', 'Small (10-49 employees)',
     5, 5, 5, 4, 5, 5, 5, 97, 'High AI Maturity'),

    ('Andreas', 'EduPro Training', 'Education & training', 'Small (10-49 employees)',
     3, 3, 3, 2, 3, 3, 2, 52, 'Medium AI Maturity');
