package internal

import (
	"crypto/rand"
	"fmt"
	"github.com/emicklei/go-restful"
	"golang.org/x/crypto/bcrypt"
	"log"
	"time"
	"../results"
)

type LoginRouting struct {

}

func (login *LoginRouting) Route(ws *restful.WebService) {
	ws.Route(ws.POST("internal/login").To(login.registerFunc))
}


func (login *LoginRouting) getFailedResult() *results.LoginResult {
	res := new(results.LoginResult)
	res.Success = false
	res.Token = ""

	return res
}

func (login *LoginRouting) registerFunc(request *restful.Request, response *restful.Response) {

	values, err := getBodyParameters(request, []string{"username", "password" })

	//FIXME variable "name" can inject SQL

	if err != nil {
		writeJsonResponse(response, login.getFailedResult(), 400)
		log.Println(err)
		return
	}

	name := values[0]
	pass := values[1]

	q := "SELECT Username, Password FROM Users WHERE Username = '" + name + "'"
	rows, err := sqlConnection.Query(q)

	if err != nil {
		writeJsonResponse(response, login.getFailedResult(), 400)
		log.Println(err)
		return
	}

	rname, hashed := "", ""
	defer rows.Close()

	if !rows.Next() {
		writeJsonResponse(response, login.getFailedResult(), 200)
		return
	}

	err = rows.Scan(&rname, &hashed)

	if err != nil {
		writeJsonResponse(response, login.getFailedResult(), 500)
		log.Println(err)
		return
	}

	if login.comparePasswords(hashed, []byte(pass)) {
		token, err := login.setOrGetToken(name)

		if err != nil {
			writeJsonResponse(response, login.getFailedResult(), 500)
			log.Println(err)
			return
		}

		res := new(results.LoginResult)
		res.Success = true
		res.Token = token
		writeJsonResponse(response, res, 200)
		return
	}

	writeJsonResponse(response, login.getFailedResult(), 200)
}

func (login *LoginRouting) setOrGetToken(username string) (string, error) {
	q := "SELECT Username, Creation, Token FROM Tokens WHERE Username = '" + username + "'"
	rows, err := sqlConnection.Query(q)
	defer rows.Close()

	if err != nil {
		return "", err
	}

	var token string
	var creation time.Time

	for rows.Next() {
		rows.Scan(&username, &creation, &token)
		return token, nil
	}

	token = login.generateToken()
	exec := "INSERT INTO Tokens (Username, Creation, Token) VALUES ('" + username + "', NOW(), '" + token + "')"
	_, err = sqlConnection.Exec(exec)
	log.Println("Inserted new token for " + username)

	return token, err
}

func (login *LoginRouting) generateToken() string {
	b := make([]byte, 50)
	rand.Read(b)
	return fmt.Sprintf("%x", b)
}

func (login *LoginRouting) comparePasswords(hashedPwd string, plainPwd []byte) bool {
	// Since we'll be getting the hashed password from the DB it
	// will be a string so we'll need to convert it to a byte slice
	byteHash := []byte(hashedPwd)
	err := bcrypt.CompareHashAndPassword(byteHash, plainPwd)

	if err != nil {
		log.Println(err)
		return false
	}

	return true
}
