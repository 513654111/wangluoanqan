from flask import Flask, render_template, request, redirect, url_for, session, make_response
import requests
import bleach

app = Flask(__name__)
app.secret_key = "verysecretflaskkey123!"

API_BASE = "https://localhost:7003/api"  # 改成你后端实际运行的端口

@app.after_request
def add_security_headers(response):
    response.headers['Content-Security-Policy'] = "default-src 'self'; style-src 'self' 'unsafe-inline'"
    response.headers['X-Content-Type-Options'] = 'nosniff'
    response.headers['X-Frame-Options'] = 'DENY'
    return response

@app.route('/')
def index():
    return '<h1>Welcome to Secure Bank</h1><a href="/login">Login</a>'

@app.route('/login', methods=['GET', 'POST'])
def login():
    if request.method == 'POST':
        username = request.form.get('username', '')
        password = request.form.get('password', '')
        totp = request.form.get('totp', '')
        try:
            resp = requests.post(f"{API_BASE}/auth/login", json={
                "username": username,
                "password": password,
                "totpCode": totp
            }, verify=False)
            if resp.ok:
                data = resp.json()
                session['token'] = data['token']
                return redirect(url_for('dashboard'))
            else:
                error = resp.json().get('message', 'Login failed')
                return render_template('login.html', error=error)
        except Exception as e:
            return render_template('login.html', error="Cannot connect to backend")
    return render_template('login.html')

@app.route('/dashboard')
def dashboard():
    return '<h1>Dashboard</h1><a href="/transfer">Transfer</a> | <a href="/logout">Logout</a>'

@app.route('/transfer', methods=['GET', 'POST'])
def transfer():
    if request.method == 'POST':
        return '<h1>Transfer Successful (Demo)</h1><a href="/dashboard">Back</a>'
    return '<h1>Transfer</h1><form method="post"><input name="account" placeholder="Account"><input name="amount" placeholder="Amount"><button type="submit">Send</button></form>'

@app.route('/logout')
def logout():
    session.clear()
    return redirect(url_for('index'))

if __name__ == '__main__':
    app.run(debug=True, ssl_context='adhoc')