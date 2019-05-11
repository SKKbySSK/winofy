package results

type LoginResult struct {
	Success bool `json:"success"`
	Token string `json:"token"`
}

