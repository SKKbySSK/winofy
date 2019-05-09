package main

type User struct {
	Username string `db:"Username"`
	Password string `db:"Password"`
}
