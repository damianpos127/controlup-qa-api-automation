# GitHub Setup Guide - Step by Step

This guide will walk you through creating a GitHub repository, pushing your code, configuring secrets, and verifying the CI/CD pipeline.

---

## Step 1: Create a GitHub Repository

### 1.1 Go to GitHub
1. Open your web browser and go to [https://github.com](https://github.com)
2. Sign in to your GitHub account (or create one if you don't have it)

### 1.2 Create New Repository
1. Click the **"+"** icon in the top right corner
2. Select **"New repository"** from the dropdown menu

### 1.3 Configure Repository Settings
Fill in the repository details:

- **Repository name**: `controlup-qa-api-automation` (or your preferred name)
- **Description** (optional): "QA Automation Framework for Binance API - ControlUp Assessment"
- **Visibility**: Select **"Public"** (required for the assessment)
- **DO NOT** check any of these boxes:
  - ❌ Add a README file (we already have one)
  - ❌ Add .gitignore (we already have one)
  - ❌ Choose a license (not needed for assessment)

### 1.4 Create Repository
1. Click the green **"Create repository"** button
2. GitHub will show you a page with setup instructions - **you can close this page** (we'll do it differently)

---

## Step 2: Initialize Git and Push Code

### 2.1 Open Terminal/PowerShell
Open your terminal or PowerShell in the project directory:
```powershell
cd "D:\Assessment\Automation QA"
```

### 2.2 Initialize Git (if not already done)
Check if git is already initialized:
```powershell
git status
```

If you see "fatal: not a git repository", initialize it:
```powershell
git init
```

### 2.3 Add All Files
Add all project files to git:
```powershell
git add .
```

**Note**: This will add all files. The `.gitignore` file will automatically exclude:
- `bin/` and `obj/` folders
- `artifacts/*` (except `.gitkeep`)
- `appsettings.Local.json` (your API key file)

### 2.4 Create Initial Commit
```powershell
git commit -m "Initial commit: ControlUp QA Automation Framework"
```

### 2.5 Add GitHub Remote
Replace `YOUR_USERNAME` with your actual GitHub username:
```powershell
git remote add origin https://github.com/YOUR_USERNAME/controlup-qa-api-automation.git
```

**Example:**
```powershell
git remote add origin https://github.com/johndoe/controlup-qa-api-automation.git
```

### 2.6 Rename Branch to Main (if needed)
```powershell
git branch -M main
```

### 2.7 Push Code to GitHub
```powershell
git push -u origin main
```

**If prompted for credentials:**
- **Username**: Your GitHub username
- **Password**: You'll need a **Personal Access Token** (not your GitHub password)
  - See "Creating a Personal Access Token" section below if needed

---

## Step 3: Create Personal Access Token (if needed)

If GitHub asks for authentication, you need a Personal Access Token:

### 3.1 Go to GitHub Settings
1. Click your profile picture (top right)
2. Click **"Settings"**

### 3.2 Create Token
1. Scroll down and click **"Developer settings"** (left sidebar)
2. Click **"Personal access tokens"**
3. Click **"Tokens (classic)"**
4. Click **"Generate new token"** → **"Generate new token (classic)"**

### 3.3 Configure Token
- **Note**: "ControlUp Assessment" (or any description)
- **Expiration**: Choose your preference (90 days is good)
- **Scopes**: Check **"repo"** (this gives full repository access)
- Click **"Generate token"** at the bottom

### 3.4 Copy Token
⚠️ **IMPORTANT**: Copy the token immediately - you won't be able to see it again!

Use this token as your password when pushing code.

---

## Step 4: Configure GitHub Secrets

GitHub Secrets are used by the CI/CD pipeline to access your RapidAPI key without exposing it in the code.

### 4.1 Go to Repository Settings
1. In your GitHub repository page, click the **"Settings"** tab (top navigation)
2. If you don't see "Settings", make sure you're the repository owner

### 4.2 Navigate to Secrets
1. In the left sidebar, click **"Secrets and variables"**
2. Click **"Actions"**

### 4.3 Add RAPIDAPI_KEY Secret
1. Click the **"New repository secret"** button
2. Fill in:
   - **Name**: `RAPIDAPI_KEY` (must be exactly this, case-sensitive)
   - **Secret**: Paste your RapidAPI key (the one from `appsettings.Local.json`)
3. Click **"Add secret"**

### 4.4 Add RAPIDAPI_HOST Secret (Optional)
1. Click **"New repository secret"** again
2. Fill in:
   - **Name**: `RAPIDAPI_HOST`
   - **Secret**: `binance43.p.rapidapi.com`
3. Click **"Add secret"**

**Note**: This is optional because it has a default value in the code, but it's good practice to set it.

### 4.5 Add RAPIDAPI_BASE_URL Secret (Optional)
1. Click **"New repository secret"** again
2. Fill in:
   - **Name**: `RAPIDAPI_BASE_URL`
   - **Secret**: `https://binance43.p.rapidapi.com`
3. Click **"Add secret"**

**Note**: This is also optional because it has a default value.

### 4.6 Verify Secrets
You should now see three secrets listed:
- ✅ `RAPIDAPI_KEY`
- ✅ `RAPIDAPI_HOST` (optional)
- ✅ `RAPIDAPI_BASE_URL` (optional)

---

## Step 5: Verify CI/CD Pipeline

### 5.1 Trigger the Pipeline
The pipeline runs automatically when you push code. Since you already pushed, it should be running. To trigger it manually:

1. Go to your repository on GitHub
2. Click the **"Actions"** tab (top navigation)
3. You should see a workflow run in progress or completed

### 5.2 Check Pipeline Status
1. Click on the workflow run to see details
2. You'll see the steps:
   - ✅ Checkout code
   - ✅ Setup .NET SDK
   - ✅ Restore dependencies
   - ✅ Build solution
   - ✅ Run tests
   - ✅ Upload test results
   - ✅ Upload test reports

### 5.3 View Test Results
1. Scroll down to the **"Run tests"** step
2. Click on it to expand
3. You should see:
   ```
   Passed! - Failed: 0, Passed: 3, Skipped: 0, Total: 3
   ```

### 5.4 Download Artifacts
1. Scroll to the bottom of the workflow run page
2. In the **"Artifacts"** section, you'll see:
   - `test-results` - Contains the generated reports
3. Click on `test-results` to download
4. Extract the zip file to see `report.json` and `report.md`

### 5.5 Troubleshooting Failed Pipeline

If the pipeline fails:

**Common Issue 1: Missing Secrets**
- Error: "401 Unauthorized" in test logs
- Solution: Go back to Step 4 and verify `RAPIDAPI_KEY` is set correctly

**Common Issue 2: Wrong Secret Names**
- Error: Tests can't find configuration
- Solution: Ensure secret names are exactly:
  - `RAPIDAPI_KEY` (not `RapidApi__Key`)
  - `RAPIDAPI_HOST` (not `RapidApi__Host`)
  - `RAPIDAPI_BASE_URL` (not `RapidApi__BaseUrl`)

**Common Issue 3: Build Errors**
- Check the "Build solution" step logs
- Ensure all files were pushed correctly

---

## Step 6: Verify Everything Works

### 6.1 Check Repository
Visit your repository URL:
```
https://github.com/YOUR_USERNAME/controlup-qa-api-automation
```

Verify:
- ✅ README.md is visible
- ✅ All source code files are present
- ✅ `.github/workflows/ci.yml` exists
- ✅ `.cursor/rules/` directory exists

### 6.2 Check CI/CD Status
1. Go to the **"Actions"** tab
2. You should see a green checkmark ✅ next to the latest workflow run
3. Click on it to see all steps completed successfully

### 6.3 Verify Artifacts
1. In the workflow run, scroll to **"Artifacts"**
2. Download `test-results`
3. Verify it contains:
   - `report.json`
   - `report.md`

---

## Quick Reference Commands

### If you need to make changes and push again:

```powershell
# Navigate to project directory
cd "D:\Assessment\Automation QA"

# Check status
git status

# Add changes
git add .

# Commit changes
git commit -m "Your commit message here"

# Push to GitHub
git push
```

### If you need to check your remote:

```powershell
# View remote URL
git remote -v

# Change remote URL (if needed)
git remote set-url origin https://github.com/YOUR_USERNAME/REPO_NAME.git
```

---

## Summary Checklist

Before submitting, verify:

- [ ] Repository is **public**
- [ ] All code is pushed to GitHub
- [ ] `RAPIDAPI_KEY` secret is configured
- [ ] CI/CD pipeline runs successfully (green checkmark ✅)
- [ ] Test artifacts are generated and downloadable
- [ ] README.md is visible and complete
- [ ] No sensitive data (API keys) in committed files

---

## Need Help?

If you encounter issues:

1. **Authentication problems**: Make sure you're using a Personal Access Token, not your password
2. **Pipeline failures**: Check the "Actions" tab for detailed error messages
3. **Missing files**: Verify `.gitignore` isn't excluding important files
4. **Secret not working**: Double-check the secret name matches exactly (case-sensitive)

---

## Next Steps

Once everything is set up:

1. ✅ Share your repository URL with the recruiter
2. ✅ The CI/CD pipeline will run automatically on every push
3. ✅ Test results and reports are available as downloadable artifacts
4. ✅ Your code is ready for review!

Good luck with your assessment! 🚀
