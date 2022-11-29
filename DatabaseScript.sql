﻿CREATE TABLE Clients
(
	WalletNumber TEXT PRIMARY KEY,
	Surname TEXT NOT NULL,
	Name TEXT NOT NULL,
	Patronymic TEXT NULL,
	Balance REAL DEFAULT 0.0 NOT NULL
);

CREATE TABLE Transactions
(
	TimeStamp TEXT NOT NULL,
	SenderWallet TEXT NOT NULL,
	RecipientWallet TEXT NOT NULL,
	Amount REAL NOT NULL,
	CurrencyType TEXT NOT NULL,
	TransactionType TEXT NOT NULL,

	PRIMARY KEY (TimeStamp, SenderWallet, RecipientWallet),
	FOREIGN KEY (SenderWallet) REFERENCES Clients(WalletNumber),
	FOREIGN KEY (RecipientWallet) REFERENCES Clients(WalletNumber)
);
