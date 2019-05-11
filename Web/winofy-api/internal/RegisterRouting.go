package internal

import (
	"github.com/emicklei/go-restful"
	"golang.org/x/crypto/bcrypt"
	"log"
	"../results"
)

type RegisterRouting struct {

}

func (register *RegisterRouting) Route(ws *restful.WebService) {
	ws.Route(ws.PUT("internal/register").To(register.registerFunc))
}

func (register *RegisterRouting) getFailedResult(messages []string) *results.RegisterResult {
	res := new(results.RegisterResult)
	res.Success = false
	res.Messages = messages

	return res
}

func (register *RegisterRouting) registerFunc(request *restful.Request, response *restful.Response) {

	evaluateUsername := func (value string) bool { return len(value) < 3 || len(value) > 20 }
	evaluatePassword := func (value string) bool { return len(value) < 5 || len(value) > 20 }

	values, err := getBodyParameters(request, []string{"username", "password" })

	if err != nil {
		writeJsonResponse(response, register.getFailedResult([]string{ results.RegisterInvalidRequest }), 400)
		log.Println(err)
		return
	}

	name := values[0]
	pass := values[1]

	if evaluateUsername(name) {
		msgs := []string{ results.RegisterIncorrectUsernameFormat }

		if evaluatePassword(pass) {
			msgs = append(msgs, results.RegisterIncorrectPasswordFormat)
		}

		writeJsonResponse(response, register.getFailedResult(msgs), 400)
		return
	}

	if evaluatePassword(pass) {
		writeJsonResponse(response, register.getFailedResult([]string{ results.RegisterIncorrectPasswordFormat }), 400)
		return
	}

	rows, err := sqlConnection.Query("SELECT Username FROM Users WHERE Username = ?", name)
	defer rows.Close()

	if err != nil {
		writeJsonResponse(response, register.getFailedResult([]string{ results.RegisterUnknown }), 500)
		log.Println(err)
		return
	}

	if rows.Next() {
		writeJsonResponse(response, register.getFailedResult([]string{ results.RegisterUserExists }), 409)
		return
	}

	hashed := register.hashAndSalt([]byte(pass))
	exec := "INSERT INTO Users (Username, Password, Creation) VALUES (?, ?, NOW())"

	_, err = sqlConnection.Exec(exec, name, hashed)

	if err != nil {
		writeJsonResponse(response, register.getFailedResult([]string{ results.RegisterUnknown }), 400)
		log.Println(err)
		return
	}

	res := new(results.RegisterResult)
	res.Success = true
	res.Messages = []string{ results.RegisterOk }
	writeJsonResponse(response, res, 200)
}

func (register *RegisterRouting) hashAndSalt(pwd []byte) string {

	// Use GenerateFromPassword to hash & salt pwd.
	// MinCost is just an integer constant provided by the bcrypt
	// package along with DefaultCost & MaxCost.
	// The cost can be any value you want provided it isn't lower
	// than the MinCost (4)
	hash, err := bcrypt.GenerateFromPassword(pwd, bcrypt.DefaultCost)
	if err != nil {
		log.Println(err)
	}
	// GenerateFromPassword returns a byte slice so we need to
	// convert the bytes to a string and return it
	return string(hash)
}