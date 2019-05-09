package main

type Connection struct {
	Username string `json:"username"`
	Password string `json:"password"`
	Database string `json:"database"`
	Address string `json:"address"`
}
