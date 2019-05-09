package main

import (
	"encoding/json"
	"flag"
	"os"
)

type Connection struct {
	Username string `json:"username"`
	Password string `json:"password"`
	Database string `json:"database"`
	Address string `json:"address"`
}

func main() {
	user := flag.String("u", "", "Username for the database")
	pass := flag.String("p", "", "Password for the database")
	db := flag.String("db", "", "Database name")
	addr := flag.String("addr", "127.0.0.1:3306", "[Optional] Address:Port. Default is 127.0.0.1:3306")
	path := flag.String("path", "db/connection.json", "[Optional] File path which will used to export json")

	flag.Parse()

	con := new(Connection)
	con.Username = *user
	con.Password = *pass
	con.Database = *db
	con.Address = *addr
	data, err := json.Marshal(con)

	if err != nil {
		println("[Error] Failed to generate json struct:")
		println(err.Error())
		return
	}

	file, err := os.Create(*path)
	defer file.Close()

	if err != nil {
		println("[Error] Failed to export the file:")
		println(err.Error())
		return
	}

	file.Write(data)
	println("json successfully exported to " + file.Name())
}
